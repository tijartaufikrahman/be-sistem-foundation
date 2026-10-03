using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
        public string? FoundationSlug { get; set; }
        public string? FoundationDomain { get; set; }
        public long? FoundationId { get; set; }
    }

    public class ForgotPasswordRequestDto
    {
        [Required]
        public string Email { get; set; } = null!;
        public string? ResetBaseUrl { get; set; }
        public string? FoundationSlug { get; set; }
        public string? FoundationDomain { get; set; }
    }

    public class VerifyOtpRequestDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Otp { get; set; } = null!;
        public string? FoundationSlug { get; set; }
        public string? FoundationDomain { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        public string? Email { get; set; }
        public string? Otp { get; set; }
        public string? Token { get; set; }
        [Required]
        public string NewPassword { get; set; } = null!;
        public string? ConfirmPassword { get; set; }
        public string? FoundationSlug { get; set; }
        public string? FoundationDomain { get; set; }
    }

    public class ResetPasswordOtpRequestDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Otp { get; set; } = null!;
        [Required]
        public string NewPassword { get; set; } = null!;
        public string? ConfirmPassword { get; set; }
    }

    public class RegisterRequestDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? FoundationSlug { get; set; }
        public string? FoundationDomain { get; set; }
        public long? FoundationId { get; set; }
    }

    public class UserAuthDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public List<long> RoleIds { get; set; } = new();
        public long? RoleId { get; set; }
        public string? RoleName { get; set; }
        public long? FoundationId { get; set; }
        public string? FoundationName { get; set; }
        public string? FoundationSlug { get; set; }
        public string? FoundationDomain { get; set; }
        public List<long> FoundationIds { get; set; } = new();
        public string? Token { get; set; }
    }
}
