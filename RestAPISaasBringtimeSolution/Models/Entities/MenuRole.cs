using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("menu_roles")]
    public class MenuRole
    {
        [Key]
        public long Id { get; set; }

        public long? MenuId { get; set; }
        public long? GroupId { get; set; }
        public long RoleId { get; set; }
        public int OrderIndex { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
