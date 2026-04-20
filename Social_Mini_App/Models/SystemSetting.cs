using System;
using System.ComponentModel.DataAnnotations;

namespace Social_Mini_App.Models
{
    public class SystemSetting
    {
        [Key]
        public Guid SettingId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
