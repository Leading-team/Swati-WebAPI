using Microsoft.Extensions.Options;
using SWWebAPI.Models;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SWWebAPI.Services
{
    public class RazorpayClient
    {
        private readonly HttpClient _httpClient;
        private readonly RazorpayOptions _options;

        public RazorpayClient(
            HttpClient httpClient,
            IOptions<RazorpayOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<JsonElement> CreateOrderAsync(
            CreateOrderRequest request)
        {
            var amountPaise = checked((int)(request.Amount * 100));

            var orderData = new
            {
                amount = amountPaise,
                currency = "INR",
                receipt = $"receipt_{Guid.NewGuid():N}"
            };

            var json = JsonSerializer.Serialize(orderData);

            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.razorpay.com/v1/orders");

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{_options.KeyId}:{_options.KeySecret}"));

            message.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            message.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            using var response =
                await _httpClient.SendAsync(message);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Razorpay returned {(int)response.StatusCode}: {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            return document.RootElement.Clone();
        }

        public bool VerifyPaymentSignature(
    string orderId,
    string paymentId,
    string signature)
        {
            if (string.IsNullOrWhiteSpace(orderId) ||
                string.IsNullOrWhiteSpace(paymentId) ||
                string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            var payload = $"{orderId}|{paymentId}";

            var keySecret = _options.KeySecret;

            var keyBytes = Encoding.UTF8.GetBytes(keySecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);

            var hashBytes = hmac.ComputeHash(payloadBytes);

            var expectedSignature =
                Convert.ToHexString(hashBytes).ToLowerInvariant();

            try
            {
                var expectedBytes =
                    Convert.FromHexString(expectedSignature);

                var actualBytes =
                    Convert.FromHexString(signature);

                return CryptographicOperations.FixedTimeEquals(
                    expectedBytes,
                    actualBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
