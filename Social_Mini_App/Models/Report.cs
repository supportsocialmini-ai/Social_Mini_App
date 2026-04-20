using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiniSocialNetwork.Models;

namespace Social_Mini_App.Models
{
    public class Report
    {
        [Key]
        public Guid ReportId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ReporterId { get; set; }
        
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }

        [Required]
        public Guid TargetId { get; set; }

        [Required]
        [MaxLength(20)]
        public string TargetType { get; set; } = "Post"; // "Post" hoặc "User"

        [Required]
        [MaxLength(100)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Resolved", "Dismissed"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedById { get; set; }
        
        [ForeignKey("ResolvedById")]
        public User? Resolver { get; set; }
    }
}
