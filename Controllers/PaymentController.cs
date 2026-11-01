using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWWebAPI.Data;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;
using SWWebAPI.Services;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SWWebAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly RazorpayClient _razorpay;
        private readonly RazorpayOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _dbContext;

        public PaymentController(
            RazorpayClient razorpay,
            IOptions<RazorpayOptions> options, HttpClient httpClient, ApplicationDbContext dbContext)
        {
            _razorpay = razorpay;
            _options = options.Value;
            _httpClient = httpClient;
            _dbContext = dbContext;
        }
        // GET: /api/payment/config
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var keyId = _options.KeyId;

            if (string.IsNullOrWhiteSpace(keyId) ||
                keyId.StartsWith("YOUR_", StringComparison.Ordinal))
            {
                return Problem(
                    detail: "Configure Razorpay:KeyId in appsettings.Development.json.");
            }

            return Ok(new { keyId });
        }
        private async Task<CreateMagazineSubscriptionResult> CreateMagazineSubscriptionAsync(
       CreateOrderRequest request,
       decimal amount,
       int countryId)
        {
            var result = new CreateMagazineSubscriptionResult();

            var connection = _dbContext.Database.GetDbConnection();

            await using var command = connection.CreateCommand();

            command.CommandText = "dbo.CreateMagazineSubscription";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@FullName", request.ShippingAddress!.FullName));
            command.Parameters.Add(new SqlParameter("@Email",
                (object?)request.ShippingAddress.Email ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Mobile", request.ShippingAddress.Phone));

            command.Parameters.Add(new SqlParameter("@Address", request.ShippingAddress.Address));
            command.Parameters.Add(new SqlParameter("@City", request.ShippingAddress.City));
            command.Parameters.Add(new SqlParameter("@State", request.ShippingAddress.State));
            command.Parameters.Add(new SqlParameter("@PostalCode", request.ShippingAddress.PostalCode));
            command.Parameters.Add(new SqlParameter("@CountryId", countryId));

            command.Parameters.Add(new SqlParameter("@PlanId", request.PlanId));

            // These values should be calculated/validated on the server.
            command.Parameters.Add(new SqlParameter("@IssueDate", DateTime.Today));
            command.Parameters.Add(new SqlParameter("@ExpiryDate", DateTime.Today.AddYears(1)));

            command.Parameters.Add(new SqlParameter("@Amount", amount));
            command.Parameters.Add(new SqlParameter("@CurrencyCode", "INR"));
            command.Parameters.Add(new SqlParameter("@PaymentMethod", DBNull.Value));

            var subscriberIdParam = new SqlParameter("@SubscriberId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var addressIdParam = new SqlParameter("@AddressId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var subscriptionIdParam = new SqlParameter("@SubscriptionId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var paymentRecordIdParam = new SqlParameter("@PaymentRecordId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(subscriberIdParam);
            command.Parameters.Add(addressIdParam);
            command.Parameters.Add(subscriptionIdParam);
            command.Parameters.Add(paymentRecordIdParam);

            bool openedConnection = connection.State != ConnectionState.Open;

            try
            {
                if (openedConnection)
                    await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                result.SubscriberId = (int)subscriberIdParam.Value;
                result.AddressId = (int)addressIdParam.Value;
                result.SubscriptionId = (int)subscriptionIdParam.Value;
                result.PaymentRecordId = (int)paymentRecordIdParam.Value;

                return result;
            }
            finally
            {
                if (openedConnection)
                    await connection.CloseAsync();
               
            }
        }
        private async Task UpdateRazorpayOrderIdAsync(
    int paymentRecordId,
    string razorpayOrderId)
        {
            var connection = _dbContext.Database.GetDbConnection();

            await using var command = connection.CreateCommand();

            command.CommandText = "dbo.UpdateRazorpayOrderId";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter(
                "@PaymentRecordId", SqlDbType.Int)
            {
                Value = paymentRecordId
            });

            command.Parameters.Add(new SqlParameter(
                "@RazorpayOrderId", SqlDbType.NVarChar, 100)
            {
                Value = razorpayOrderId
            });

            bool openedConnection = connection.State != ConnectionState.Open;

            try
            {
                if (openedConnection)
                    await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (openedConnection)
                    await connection.CloseAsync();
            }
        }
        private async Task<RazorpayPaymentDetails> GetPaymentAsync(
    string paymentId)
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{_options.KeyId}:{_options.KeySecret}"
                )
            );

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.razorpay.com/v1/payments/{Uri.EscapeDataString(paymentId)}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            using var response = await _httpClient.SendAsync(request);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Razorpay payment fetch failed: {responseContent}"
                );
            }

            return JsonSerializer.Deserialize<RazorpayPaymentDetails>(
                responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? throw new Exception("Unable to retrieve payment details.");
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");
            try
            {
                decimal amount = await GetSubscriptionAmountAsync(request.PlanId);
                // 2. Resolve CountryId using your country table
                int countryId = 1; //await GetCountryIdAsync(request.CountryCode);

                // 3. Insert all subscription records
                var subscription = await CreateMagazineSubscriptionAsync(
                    request,
                    amount,
                    countryId);

                var order = await _razorpay.CreateOrderAsync(request);
                
                // Get Razorpay Order ID
                string? razorpayOrderId = order.GetProperty("id").GetString();

                if (string.IsNullOrWhiteSpace(razorpayOrderId))
                {
                    throw new Exception("Razorpay Order ID was not returned.");
                }

                // 5. Save Razorpay Order ID
                await UpdateRazorpayOrderIdAsync(
                    subscription.PaymentRecordId,
                    razorpayOrderId);
                return Ok(new
                {
                    orderId = order.GetProperty("id").GetString(),
                    amount = order.GetProperty("amount").GetInt32(),
                    currency = order.GetProperty("currency").GetString(),
                    keyId = _options.KeyId
                });
            }
            catch (Exception ex)
            {
                // Log ex using ILogger
                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to create subscription payment order."
                });
            }
        }
        private async Task UpdateSubscriptionPaymentAsync(
    int paymentRecordId,
    string razorpayPaymentId,
    string paymentStatus,
    string? paymentMethod,
    bool signatureVerified)
        {
            var connection = _dbContext.Database.GetDbConnection();

            await using var command = connection.CreateCommand();

            command.CommandText = "dbo.UpdateSubscriptionPayment";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter(
                "@PaymentRecordId", SqlDbType.Int)
            {
                Value = paymentRecordId
            });

            command.Parameters.Add(new SqlParameter(
                "@RazorpayPaymentId", SqlDbType.NVarChar, 100)
            {
                Value = razorpayPaymentId
            });

            command.Parameters.Add(new SqlParameter(
                "@PaymentStatus", SqlDbType.VarChar, 30)
            {
                Value = paymentStatus
            });

            command.Parameters.Add(new SqlParameter(
                "@PaymentMethod", SqlDbType.VarChar, 50)
            {
                Value = (object?)paymentMethod ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter(
                "@SignatureVerified", SqlDbType.Bit)
            {
                Value = signatureVerified
            });

            bool openedConnection =
                connection.State != ConnectionState.Open;

            try
            {
                if (openedConnection)
                    await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (openedConnection)
                    await connection.CloseAsync();
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment(
        [FromBody] VerifyPaymentRequest request)
        {
            try
            {
                // 1. Verify Razorpay signature
                var isValid = _razorpay.VerifyPaymentSignature(
            request.OrderId,
            request.PaymentId,
            request.Signature
        );

                if (!isValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment signature verification failed."
                    });
                }
                // 2. Get our payment record using Razorpay Order ID
                var paymentRecord = await GetPaymentRecordByOrderIdAsync(
           request.OrderId);

                if (paymentRecord == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Payment record not found."
                    });
                }

                // 3. Fetch actual payment details from Razorpay
                var payment = await GetPaymentAsync(
                    request.PaymentId);

                if (string.IsNullOrWhiteSpace(payment.OrderId))
                {
                    throw new Exception("Razorpay payment response does not contain Order ID.");
                }

                // 4. Validate order ID
                if (payment.OrderId != request.OrderId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment Order ID mismatch."
                    });
                }

                // 5. Validate amount (Razorpay amount is in paise)
                long expectedAmountPaise = decimal.ToInt64(
                    paymentRecord.Amount * 100);

                if (payment.Amount != expectedAmountPaise)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment amount mismatch."
                    });
                }

                // 6. Validate currency
                if (!string.Equals(
                    payment.Currency,
                    paymentRecord.CurrencyCode,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment currency mismatch."
                    });
                }

                // 7. Update payment and subscription status
                string paymentStatus = payment.Status.ToLowerInvariant();

                await UpdateSubscriptionPaymentAsync(
                    paymentRecord.subscription_payment_id,
                    request.PaymentId,
                    paymentStatus,
                    payment.Method,
                    true
                );

                // 8. Return result
                if (paymentStatus != "captured")
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Payment status: {paymentStatus}. Subscription is not activated.",
                        orderId = request.OrderId,
                        paymentId = request.PaymentId
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Payment signature verified successfully.",
                    orderId = request.OrderId,
                    paymentId = request.PaymentId
                });
            }
            catch (Exception ex)
            {
                // Log exception using ILogger
                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to verify payment."
                });
            }
        }
        private async Task<decimal> GetSubscriptionAmountAsync(int planId)
        {
            var plan = await _dbContext.planprice
                .FirstOrDefaultAsync(x => x.plan_id == planId);

            if (plan == null)
                throw new Exception("Subscription plan not found.");

            return plan.price + plan.courier_price;
        }

        private async Task<SubscriptionPayment> GetPaymentRecordByOrderIdAsync(string? orderId)
        {
            var paymentRecord = await _dbContext.subscription_payment
                .FirstOrDefaultAsync(x => x.RazorpayOrderId == orderId);

            if (paymentRecord == null)
                throw new Exception("Payment record not found.");

            return paymentRecord;
        }

        public class CreateMagazineSubscriptionResult
        {
            public int SubscriberId { get; set; }
            public int AddressId { get; set; }
            public int SubscriptionId { get; set; }
            public int PaymentRecordId { get; set; }
        }
       

        public class VerifyPaymentRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public string PaymentId { get; set; } = string.Empty;
            public string Signature { get; set; } = string.Empty;
        }
        public class RazorpayPaymentDetails
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = "";

            [JsonPropertyName("order_id")]
            public string OrderId { get; set; } = "";

            [JsonPropertyName("amount")]
            public long Amount { get; set; }

            [JsonPropertyName("currency")]
            public string Currency { get; set; } = "";

            [JsonPropertyName("status")]
            public string Status { get; set; } = "";

            [JsonPropertyName("method")]
            public string Method { get; set; } = "";
        }

    }
}

