namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class UserRoleDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = string.Empty;
        public long RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string RoleDisplayName { get; set; } = string.Empty;
        public DateTime? Created_at { get; set; }
    }

    public class CreateUserRoleDto
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
    }
}
