using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace CrestBank.Http
{
    public class BankHttpClient : IBankHttpClient
    {
        private readonly HttpClient _client;

        private readonly string _sharedSecret;

        public BankHttpClient(string baseAddress, string sharedSecret)
        {
            _sharedSecret = sharedSecret;

            _client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(3)
            };
        }

        public async Task<(bool Success, string Message)> TransferAsync(TransferRequestDto transfer)
        {
            string json = JsonSerializer.Serialize(transfer);

            // Signed per attempt, so the retry carries a fresh signature over the same body.
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "transfers")
            {
                Content = content
            };

            request.Headers.Add("X-Bank-Timestamp", timestamp);
            request.Headers.Add(
                "X-Bank-Signature",
                RequestSignature.Sign(_sharedSecret, timestamp, json));

            try
            {
                using HttpResponseMessage response = await _client.SendAsync(request);

                string message = await response.Content.ReadAsStringAsync();

                // 401 is a credentials problem, not a transfer problem: say so plainly.
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, "Recipient bank rejected the request signature.");
                }

                return (response.IsSuccessStatusCode, message);
            }
            catch (HttpRequestException)
            {
                // The other bank is not running, or refused the connection.
                return (false, "Recipient bank is unreachable. Nothing was sent.");
            }
            catch (TaskCanceledException)
            {
                // The other bank accepted the connection but never answered.
                return (false, "Recipient bank did not answer, so the outcome is unknown. "
                             + "Check with the bank before retrying.");
            }
        }

        public async Task<(bool Success, string Message)> BuyAirtimeAsync(AirtimeRequestDto order)
        {
            string json = JsonSerializer.Serialize(order);

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            string path = order.Product == "data" ? "data" : "airtime";

            using var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };

            request.Headers.Add("X-Bank-Timestamp", timestamp);
            request.Headers.Add(
                "X-Bank-Signature",
                RequestSignature.Sign(_sharedSecret, timestamp, json));

            try
            {
                using HttpResponseMessage response = await _client.SendAsync(request);

                string message = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, "Bank A rejected the request signature.");
                }

                return (response.IsSuccessStatusCode, message);
            }
            catch (HttpRequestException)
            {
                return (false, "Bank A is unreachable, so nothing was delivered.");
            }
            catch (TaskCanceledException)
            {
                return (false, "Bank A did not answer, so nothing was delivered.");
            }
        }
    }
}
