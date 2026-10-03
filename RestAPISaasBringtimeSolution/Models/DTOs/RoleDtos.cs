using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class RoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CreateRoleDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class EditRoleDto
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
