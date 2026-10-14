using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SWWebAPI.Models;
using SWWebAPI.Services;

namespace SWWebAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly RazorpayClient _razorpay;
        private readonly RazorpayOptions _options;

        public PaymentController(
            RazorpayClient razorpay,
            IOptions<RazorpayOptions> options)
        {
            _razorpay = razorpay;
            _options = options.Value;
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
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request)
        {
            var order = await _razorpay.CreateOrderAsync(request);

            return Ok(new
            {
                orderId = order.GetProperty("id").GetString(),
                amount = order.GetProperty("amount").GetInt32(),
                currency = order.GetProperty("currency").GetString(),
                keyId = _options.KeyId
            });
        }

        [HttpPost("verify")]
        public  IActionResult VerifyPayment(
        [FromBody] VerifyPaymentRequest request)
        {
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

            return Ok(new
            {
                success = true,
                message = "Payment signature verified successfully.",
                orderId = request.OrderId,
                paymentId = request.PaymentId
            });
        }
        public class VerifyPaymentRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public string PaymentId { get; set; } = string.Empty;
            public string Signature { get; set; } = string.Empty;
        }
    

}
}

