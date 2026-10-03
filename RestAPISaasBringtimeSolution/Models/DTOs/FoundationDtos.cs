using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class FoundationDto
    {
        public long Id { get; set; }
        public long? FoundationTypeId { get; set; }
        public string? FoundationTypeName { get; set; }
        public long? UserId { get; set; }
        public string? AdminName { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Domain { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? WhatsappNumber { get; set; }
        public string? LegalPermitNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountHolder { get; set; }
        public bool IsActive { get; set; }
        public List<long> UserIds { get; set; } = new();
        public List<string> AdminNames { get; set; } = new();
    }

    public class CreateFoundationDto
    {
        public long? FoundationTypeId { get; set; }
        public long? UserId { get; set; }
        public List<long>? UserIds { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Slug { get; set; } = null!;
        public string? Domain { get; set; }
        [Required]
        public string LegalPermitNumber { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? WhatsappNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountHolder { get; set; }
    }

    public class EditFoundationDto
    {
        public long Id { get; set; }
        public long? FoundationTypeId { get; set; }
        public long? UserId { get; set; }
        public List<long>? UserIds { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Slug { get; set; } = null!;
        public string? Domain { get; set; }
        public string? LegalPermitNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? WhatsappNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountHolder { get; set; }
        public bool IsActive { get; set; }
    }
}
