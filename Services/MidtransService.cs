using DeWaveFreeAPI.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DeWaveFreeAPI.Services
{
    public record MidtransResult(string SnapToken, string OrderId);

    public interface IMidtransService
    {
        Task<MidtransResult> CreateSnapTokenAsync(string orderId, decimal amount, string customerName, string customerEmail);
        Task<string> GetTransactionStatusAsync(string orderId);
        bool VerifySignature(string orderId, string statusCode, string grossAmount, string receivedSignature);
    }
    public class MidtransService : IMidtransService
    {
        private readonly MidtransSettings _midtrans;
        private readonly HttpClient _http;
        private readonly ILogger<MidtransService> _logger;

        public MidtransService(
            IOptions<MidtransSettings> midtransOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<MidtransService> logger)
        {
            _midtrans = midtransOptions.Value;
            _http = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<string> GetTransactionStatusAsync(string orderId)
        {
            var statusUrl = _midtrans.IsProduction
                ? $"https://api.midtrans.com/v2/{orderId}/status"
                : $"https://api.sandbox.midtrans.com/v2/{orderId}/status";

            var request = new HttpRequestMessage(HttpMethod.Get, statusUrl);

            var auth = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(_midtrans.ServerKey + ":"));

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            HttpResponseMessage response;
            string json;

            try
            {
                response = await _http.SendAsync(request);
                json = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Midtrans status check failed for order {OrderId}", orderId);
                throw new PaymentGatewayException("Payment gateway unreachable.");
            }

            try
            {
                var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("transaction_status").GetString()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse status response for order {OrderId}", orderId);
                throw new PaymentGatewayException("Unexpected response from payment gateway.");
            }
        }

        public async Task<MidtransResult> CreateSnapTokenAsync(
            string orderId,
            decimal amount,
            string customerName,
            string customerEmail)
        {
            var payload = new
            {
                transaction_details = new
                {
                    order_id = orderId,
                    gross_amount = amount
                },
                customer_details = new
                {
                    first_name = customerName,
                    email = customerEmail
                }
            };

            var snapUrl = _midtrans.IsProduction
                ? "https://api.midtrans.com/snap/v1/transactions"
                : "https://api.sandbox.midtrans.com/snap/v1/transactions";

            var request = new HttpRequestMessage(HttpMethod.Post, snapUrl);

            var auth = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(_midtrans.ServerKey + ":"));

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response;
            string json;

            try
            {
                response = await _http.SendAsync(request);
                json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Midtrans response: {StatusCode} - {Body}",
                    (int)response.StatusCode, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Midtrans HTTP call failed for order {OrderId}", orderId);
                throw new PaymentGatewayException("Payment gateway unreachable. Please try again.");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Midtrans rejected order {OrderId}: {StatusCode} - {Body}",
                    orderId, (int)response.StatusCode, json);
                throw new PaymentGatewayException("Payment gateway error. Please try again.");
            }

            string? snapToken;
            try
            {
                var snapResponse = JsonDocument.Parse(json);
                snapToken = snapResponse.RootElement
                    .GetProperty("token").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Midtrans response for order {OrderId}", orderId);
                throw new PaymentGatewayException("Unexpected response from payment gateway.");
            }

            if (string.IsNullOrEmpty(snapToken))
            {
                _logger.LogError("Midtrans returned empty snap token for order {OrderId}", orderId);
                throw new PaymentGatewayException("Payment gateway returned an invalid token.");
            }

            return new MidtransResult(snapToken, orderId);
        }

        public bool VerifySignature(
            string orderId,
            string statusCode,
            string grossAmount,
            string receivedSignature)
        {
            var raw = orderId + statusCode + grossAmount + _midtrans.ServerKey;
            var hashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(raw));
            var expected = Convert.ToHexString(hashBytes).ToLower();

            return expected == receivedSignature.ToLower();
        }
    }
}
