using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Social_Mini_App.Helpers;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

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
            var vnpay = new VnpayLibrary();
            var vnpayConfig = _configuration.GetSection("Vnpay");

            string tmnCode = vnpayConfig["TmnCode"] ?? "";
            string hashSecret = vnpayConfig["HashSecret"] ?? "";
            string baseUrl = vnpayConfig["BaseUrl"] ?? "";

            var ipAddress = GetIpAddress(context);
            // Chuẩn hóa IP: Lấy đoạn cuối nếu là IPv6 mapped, nếu là ::1 thì đổi về 127.0.0.1
            var finalIp = ipAddress.Contains(":") ? ipAddress.Split(':').Last() : ipAddress;
            if (finalIp == "1") finalIp = "127.0.0.1";

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)Math.Round(payment.Amount * 100)).ToString()); 
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", finalIp); 
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", payment.OrderInfo ?? "Thanh toan don hang");
            vnpay.AddRequestData("vnp_OrderType", "other"); // Theo mẫu anh gửi là 'other'
            var request = context.Request;
            var returnUrl = $"{request.Scheme}://{request.Host}/api/Payment/vnpay-return";
            
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", payment.OrderId);

            string paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
            return paymentUrl;
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
