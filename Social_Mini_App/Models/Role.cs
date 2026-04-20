using System.ComponentModel.DataAnnotations;

namespace MiniSocialNetwork.Models
{
    public class Role
    {
        [Key]
        public Guid RoleId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation property for Join Table
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
