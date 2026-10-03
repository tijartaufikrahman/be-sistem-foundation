using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        [MaxLength(255)]
        public string? AvatarUrl { get; set; }

        [MaxLength(50)]
        public string AuthProvider { get; set; } = "local";

        [MaxLength(255)]
        public string? ProviderId { get; set; }

        [MaxLength(255)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
