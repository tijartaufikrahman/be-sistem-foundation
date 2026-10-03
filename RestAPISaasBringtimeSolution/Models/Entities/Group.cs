using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("groups")]
    public class Group
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? IconClass { get; set; }

        public int OrderIndex { get; set; } = 0;

        public string? Note { get; set; }

        public bool IsOpenDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        // Audit Logs
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
