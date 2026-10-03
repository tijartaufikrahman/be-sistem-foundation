using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string AuthProvider { get; set; } = null!;
        public bool IsActive { get; set; }
        public string Roles { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string? ConfirmPassword { get; set; }
        public string? PhoneNumber { get; set; }
        public long? RoleId { get; set; }
    }

    public class EditUserDto
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public long? RoleId { get; set; }
    }
}
