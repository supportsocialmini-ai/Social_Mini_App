using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Social_Mini_App.Helpers
{
    public class VnpayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(StringComparer.Ordinal);
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(StringComparer.Ordinal);

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var signDataBuilder = new StringBuilder();

            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    // Theo mẫu SchneeJob: Gửi đi thì Encode (WebUtility -> dấu +)
                    signDataBuilder.Append(VnpayEncode(kv.Key) + "=" + VnpayEncode(kv.Value) + "&");
                }
            }

            var signData = signDataBuilder.ToString();
            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            Console.WriteLine("DEBUG - VNPay signData: " + signData);

            var vnp_SecureHash = HmacSha512(vnp_HashSecret, signData);
            var paymentUrl = baseUrl + "?" + signData + "&vnp_SecureHash=" + vnp_SecureHash;

            return paymentUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var data = new StringBuilder();
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHashType" && kv.Key != "vnp_SecureHash")
                {
                    // SchneeJob logic: Không encode khi validate callback
                    data.Append(kv.Key + "=" + kv.Value + "&");
                }
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            // Debug để kiểm tra chuỗi nhận về từ VNPay
            Console.WriteLine("DEBUG - VNPay Callback Data: " + data.ToString());

            var checkSum = HmacSha512(secretKey, data.ToString());
            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string VnpayEncode(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return WebUtility.UrlEncode(str);
        }

        private string HmacSha512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }
        }
    }
}
