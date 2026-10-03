using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("navbar_groups")]
    public class NavbarGroup
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("icon_class")]
        public string? IconClass { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        [Column("created_at")]
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public long? Created_by { get; set; }

        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }

        [Column("updated_by")]
        public long? Updated_by { get; set; }
    }
}
