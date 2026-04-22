using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiniSocialNetwork.Models;

namespace Social_Mini_App.Models
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public Guid? PackageId { get; set; } // Liên kết tới gói cụ thể

        [Required]
        [MaxLength(20)]
        public string Tier { get; set; } = "Standard"; // Standard, Premium

        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PackageId")]
        public virtual SubscriptionPackage? Package { get; set; }
    }
}
