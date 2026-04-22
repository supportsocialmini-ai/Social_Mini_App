using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Social_Mini_App.Helpers;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using System.Linq;

namespace Social_Mini_App.Services
{
    public class VnpayService : IVnpayService
    {
        private readonly IConfiguration _configuration;

        public VnpayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(HttpContext context, Payment payment)
        {
            var vnpayConfig = _configuration.GetSection("Vnpay");

            string tmnCode = vnpayConfig["TmnCode"] ?? "";
            string hashSecret = vnpayConfig["HashSecret"] ?? "";
            string baseUrl = vnpayConfig["BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

            var ipAddress = GetIpAddress(context);
            var finalIp = ipAddress.Contains(":") ? ipAddress.Split(':').Last() : ipAddress;
            if (finalIp == "1") finalIp = "127.0.0.1";

            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = ((long)Math.Round(payment.Amount * 100)).ToString(),
                ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
                ["vnp_CurrCode"] = "VND",
                ["vnp_IpAddr"] = finalIp,
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = payment.OrderInfo?.Replace(" ", "-") ?? "Thanh-toan",
                ["vnp_OrderType"] = "250000", // Đổi từ 'other' sang mã chuẩn 250000
                ["vnp_ReturnUrl"] = $"{context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? context.Request.Scheme}://{context.Request.Host}/api/Payment/vnpay-return",
                ["vnp_TxnRef"] = payment.OrderId
            };

            // Tạo chuỗi data để băm (giống SchneeJob)
            var signData = string.Join("&", vnpParams
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => $"{System.Net.WebUtility.UrlEncode(kvp.Key)}={System.Net.WebUtility.UrlEncode(kvp.Value)}"));

            string secureHash;
            using (var hmac = new System.Security.Cryptography.HMACSHA512(System.Text.Encoding.UTF8.GetBytes(hashSecret)))
            {
                var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signData));
                secureHash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }

            return $"{baseUrl}?{signData}&vnp_SecureHash={secureHash}";
        }

        private string GetIpAddress(HttpContext context)
        {
            string ipAddress = string.Empty;
            try
            {
                // 1. Ưu tiên lấy IP từ cấu hình Test (dùng cho local test với Public IP theo yêu cầu)
                string testIp = _configuration["Vnpay:TestIpAddress"];
                if (!string.IsNullOrEmpty(testIp))
                {
                    return testIp;
                }

                // 2. Ưu tiên lấy từ header X-Forwarded-For (Render/Reverse Proxy)
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = context.Connection.RemoteIpAddress?.ToString();
                }
            }
            catch
            {
                ipAddress = "127.0.0.1";
            }
            return ipAddress ?? "127.0.0.1";
        }

        public bool ValidateCallback(IQueryCollection collections)
        {
            var vnpay = new VnpayLibrary();
            var hashSecret = _configuration["Vnpay:HashSecret"] ?? "";

            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value!);
                }
            }

            string vnp_SecureHash = collections["vnp_SecureHash"]!;
            bool isValid = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);

            return isValid;
        }
    }
}
