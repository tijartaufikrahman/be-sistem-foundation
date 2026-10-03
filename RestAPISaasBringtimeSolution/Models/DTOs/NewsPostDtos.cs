using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class NewsPostDto
    {
        public long Id { get; set; }
        public long FoundationId { get; set; }
        public string FoundationName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? ThumbnailUrl { get; set; }
        public string Content { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int ViewCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateNewsPostDto
    {
        [Required]
        public long FoundationId { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Slug { get; set; } = null!;
        public string? ThumbnailUrl { get; set; }
        [Required]
        public string Content { get; set; } = null!;
        [Required]
        public string Category { get; set; } = null!;
    }

    public class EditNewsPostDto
    {
        public long Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Slug { get; set; } = null!;
        public string? ThumbnailUrl { get; set; }
        [Required]
        public string Content { get; set; } = null!;
        [Required]
        public string Category { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
