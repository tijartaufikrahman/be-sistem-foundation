using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("foundations")]
    public class Foundation
    {
        [Key]
        public long Id { get; set; }

        public long? FoundationTypeId { get; set; }

        public long? UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Domain { get; set; }

        [MaxLength(255)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? BannerUrl { get; set; }

        public string? Bio { get; set; }
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? WhatsappNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string LegalPermitNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(150)]
        public string? BankAccountHolder { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
