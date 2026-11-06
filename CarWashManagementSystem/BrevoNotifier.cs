using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CarWashManagementSystem
{
    internal static class BrevoNotifier
    {
        private const string Endpoint = "https://api.brevo.com/v3/smtp/email";
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<string> SendReadyEmailAsync(
            string customerName,
            string customerEmail,
            string carName,
            string serviceInformation)
        {
            if (String.IsNullOrWhiteSpace(customerEmail)) return null;

            string apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");
            if (String.IsNullOrWhiteSpace(apiKey))
                apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY", EnvironmentVariableTarget.User);
            if (String.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("BREVO_API_KEY is not set in the Windows environment.");

            string safeName = String.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName;
            string safeCar = String.IsNullOrWhiteSpace(carName) ? "car" : carName;
            string body = "Hello " + safeName + ",\n\n" +
                          "Great news! Your " + safeCar + " is ready for pickup.\n\n" +
                          "Your car wash service has been completed successfully.";
            if (!String.IsNullOrWhiteSpace(serviceInformation))
                body += "\n\nService: " + serviceInformation;
            body += "\n\nThank you for choosing our Car Wash Center!";

            string payload = "{\"sender\":{\"name\":\"Car Wash System\",\"email\":\"asmae3629@gmail.com\"}," +
                "\"to\":[{\"email\":\"" + JsonEscape(customerEmail.Trim()) + "\",\"name\":\"" + JsonEscape(safeName) + "\"}]," +
                "\"subject\":\"Your Car Is Ready!\",\"textContent\":\"" + JsonEscape(body) + "\"}";

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Endpoint))
            {
                request.Headers.TryAddWithoutValidation("api-key", apiKey);
                request.Headers.TryAddWithoutValidation("accept", "application/json");
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false))
                {
                    string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("Brevo returned HTTP " + (int)response.StatusCode +
                            " (" + response.ReasonPhrase + "). " + responseBody.Replace(apiKey, "[REDACTED]"));
                    return responseBody;
                }
            }
        }

        private static string JsonEscape(string value)
        {
            StringBuilder escaped = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                switch (character)
                {
                    case '\\': escaped.Append("\\\\"); break;
                    case '"': escaped.Append("\\\""); break;
                    case '\b': escaped.Append("\\b"); break;
                    case '\f': escaped.Append("\\f"); break;
                    case '\n': escaped.Append("\\n"); break;
                    case '\r': escaped.Append("\\r"); break;
                    case '\t': escaped.Append("\\t"); break;
                    default:
                        if (Char.IsControl(character))
                            escaped.Append("\\u").Append(((int)character).ToString("x4"));
                        else
                            escaped.Append(character);
                        break;
                }
            }
            return escaped.ToString();
        }
    }
}
