using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiniSocialNetwork.Models;

namespace Social_Mini_App.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public Guid? PackageId { get; set; } // Liên kết tới gói dịch vụ

        [MaxLength(100)]
        public string? OrderId { get; set; } // Mã đơn hàng của hệ thống mình

        [MaxLength(200)]
        public string? OrderInfo { get; set; } // Nội dung thanh toán

        [MaxLength(100)]
        public string? VnpayTranId { get; set; } // Mã giao dịch trả về từ VNPay

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PackageId")]
        public virtual SubscriptionPackage? Package { get; set; }
    }
}
