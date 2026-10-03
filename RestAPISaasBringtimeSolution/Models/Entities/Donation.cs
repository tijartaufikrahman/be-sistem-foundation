using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISaasBringtimeSolution.Models.Entities
{
    [Table("donations")]
    public class Donation
    {
        [Key]
        public long Id { get; set; }

        public long CampaignId { get; set; }
        public long? UserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DonorName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DonorEmail { get; set; }

        [MaxLength(20)]
        public string? DonorPhone { get; set; }

        public bool IsAnonymous { get; set; } = false;

        public decimal Amount { get; set; }
        public decimal PlatformTip { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(30)]
        public string PaymentStatus { get; set; } = "pending";

        [MaxLength(100)]
        public string? MidtransOrderId { get; set; }

        [MaxLength(100)]
        public string? MidtransTransactionId { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? PrayerMessage { get; set; }
        public int AamiinCount { get; set; } = 0;

        // Audit Logs (Standalone / Non-relational)
        public DateTime? Created_at { get; set; } = DateTime.UtcNow;
        public long? Created_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public long? Updated_by { get; set; }
    }
}
