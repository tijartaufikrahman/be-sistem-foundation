using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class DonationDto
    {
        public long Id { get; set; }
        public long CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public string FoundationName { get; set; } = null!;
        public long? UserId { get; set; }
        public string DonorName { get; set; } = null!;
        public string? DonorEmail { get; set; }
        public string? DonorPhone { get; set; }
        public bool IsAnonymous { get; set; }
        public decimal Amount { get; set; }
        public decimal PlatformTip { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string InvoiceNumber { get; set; } = null!;
        public string? MidtransOrderId { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PrayerMessage { get; set; }
        public int AamiinCount { get; set; }
    }

    public class CreateDonationDto
    {
        [Required]
        public long CampaignId { get; set; }
        public long? UserId { get; set; }
        public string? DonorName { get; set; }
        public string? DonorEmail { get; set; }
        public string? DonorPhone { get; set; }
        public bool IsAnonymous { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public decimal PlatformTip { get; set; }
        public string? PrayerMessage { get; set; }
    }
}
