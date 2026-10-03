using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("news_posts")]
    public class NewsPost
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

        [MaxLength(255)]
        public string? ThumbnailUrl { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = "news";

        public int ViewCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
