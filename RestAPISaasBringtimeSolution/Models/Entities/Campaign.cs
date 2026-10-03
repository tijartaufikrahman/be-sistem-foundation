using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("campaigns")]
    public class Campaign
    {
        [Key]
        public long Id { get; set; }

        public long FoundationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = "charity";

        public decimal TargetAmount { get; set; }

        [NotMapped]
        public decimal CollectedAmount { get; set; } = 0;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [MaxLength(255)]
        public string? BannerUrl { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
