using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("approval_roles")]
    public class ApprovalRole
    {
        [Key]
        public long Id { get; set; }

        public long ApprovalId { get; set; }
        public long RoleId { get; set; }
        public int StepLevel { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
