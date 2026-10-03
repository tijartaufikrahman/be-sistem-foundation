using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("menus")]
    public class Menu
    {
        [Key]
        public long Id { get; set; }

        public long? FoundationTypeId { get; set; }

        public long? ParentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Label { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string RouteUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? IconClass { get; set; }

        public int OrderIndex { get; set; } = 0;

        public string? Note { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
