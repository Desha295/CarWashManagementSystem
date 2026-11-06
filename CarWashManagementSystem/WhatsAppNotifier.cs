using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace CarWashManagementSystem
{
    internal static class WhatsAppNotifier
    {
        public static bool SendReadyMessage(string phone, string customerName, string car)
        {
            string token = ConfigurationManager.AppSettings["WhatsAppAccessToken"];
            string phoneNumberId = ConfigurationManager.AppSettings["WhatsAppPhoneNumberId"];
            string templateName = ConfigurationManager.AppSettings["WhatsAppReadyTemplate"];
            string language = ConfigurationManager.AppSettings["WhatsAppTemplateLanguage"];
            string graphVersion = ConfigurationManager.AppSettings["WhatsAppGraphVersion"];
            if (String.IsNullOrWhiteSpace(phone) || String.IsNullOrWhiteSpace(token) ||
                String.IsNullOrWhiteSpace(phoneNumberId) || String.IsNullOrWhiteSpace(templateName))
                return false;

            string recipient = DigitsOnly(phone);
            // Customer records commonly contain Egyptian local mobile numbers (01...).
            if (recipient.Length == 11 && recipient.StartsWith("01", StringComparison.Ordinal))
                recipient = "2" + recipient;
            if (recipient.Length < 7) return false;

            string url = "https://graph.facebook.com/" + Uri.EscapeDataString(String.IsNullOrWhiteSpace(graphVersion) ? "v22.0" : graphVersion) + "/" +
                         Uri.EscapeDataString(phoneNumberId) + "/messages";
            string payload = "{\"messaging_product\":\"whatsapp\",\"to\":\"" + JsonEscape(recipient) +
                "\",\"type\":\"template\",\"template\":{\"name\":\"" + JsonEscape(templateName) +
                "\",\"language\":{\"code\":\"" + JsonEscape(String.IsNullOrWhiteSpace(language) ? "en_US" : language) +
                "\"},\"components\":[{\"type\":\"body\",\"parameters\":[" +
                "{\"type\":\"text\",\"text\":\"" + JsonEscape(String.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName) + "\"}," +
                "{\"type\":\"text\",\"text\":\"" + JsonEscape(String.IsNullOrWhiteSpace(car) ? "your car" : car) + "\"}" +
                "]}]}}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;
            request.Timeout = 15000;
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            request.ContentLength = bytes.Length;
            using (Stream stream = request.GetRequestStream()) stream.Write(bytes, 0, bytes.Length);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
        }

        private static string DigitsOnly(string value)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in value) if (Char.IsDigit(c)) result.Append(c);
            return result.ToString();
        }

        private static string JsonEscape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}
