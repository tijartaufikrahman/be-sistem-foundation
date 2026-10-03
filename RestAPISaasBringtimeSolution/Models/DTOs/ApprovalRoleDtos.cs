namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class ApprovalRoleDto
    {
        public long Id { get; set; }
        public long ApprovalId { get; set; }
        public string ApprovalName { get; set; } = null!;
        public long RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public int StepLevel { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateApprovalRoleDto
    {
        public long ApprovalId { get; set; }
        public long RoleId { get; set; }
        public int StepLevel { get; set; }
    }

    public class EditApprovalRoleDto
    {
        public long Id { get; set; }
        public long ApprovalId { get; set; }
        public long RoleId { get; set; }
        public int StepLevel { get; set; }
        public bool IsActive { get; set; }
    }
}
