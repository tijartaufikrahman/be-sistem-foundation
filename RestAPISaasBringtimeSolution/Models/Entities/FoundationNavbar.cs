using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("foundation_navbars")]
    public class FoundationNavbar
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("foundation_id")]
        public long FoundationId { get; set; }

        [Column("navbar_id")]
        public long? NavbarId { get; set; }

        [Column("navbar_group_id")]
        public long? NavbarGroupId { get; set; }

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
