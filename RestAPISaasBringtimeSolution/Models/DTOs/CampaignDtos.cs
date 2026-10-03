using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class CampaignDto
    {
        public long Id { get; set; }
        public long FoundationId { get; set; }
        public string FoundationName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? BannerUrl { get; set; }
        public string Description { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class CreateCampaignDto
    {
        [Required]
        public long FoundationId { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Slug { get; set; } = null!;
        [Required]
        public string Category { get; set; } = null!;
        [Required]
        public decimal TargetAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? BannerUrl { get; set; }
        [Required]
        public string Description { get; set; } = null!;
    }

    public class EditCampaignDto
    {
        public long Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Slug { get; set; } = null!;
        [Required]
        public string Category { get; set; } = null!;
        [Required]
        public decimal TargetAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? BannerUrl { get; set; }
        [Required]
        public string Description { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
