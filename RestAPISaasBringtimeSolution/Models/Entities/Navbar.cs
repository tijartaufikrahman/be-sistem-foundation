using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("navbars")]
    public class Navbar
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("navbar_group_id")]
        public long? NavbarGroupId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("url")]
        public string Url { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("icon_class")]
        public string? IconClass { get; set; }

        [MaxLength(10)]
        [Column("target")]
        public string Target { get; set; } = "_self";

        [MaxLength(20)]
        [Column("platform")]
        public string Platform { get; set; } = "all";

        [Column("order_index")]
        public int OrderIndex { get; set; } = 0;

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
