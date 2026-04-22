using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class SubscriptionPackage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Ví dụ: Premium

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }
        
        public string? Features { get; set; } // Danh sách tính năng, cách nhau bằng dấu phẩy
        
        public int DurationDays { get; set; } = 30; // Số ngày hiệu lực (Mặc định 30 ngày)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
}
