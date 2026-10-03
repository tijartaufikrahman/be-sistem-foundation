using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class ApprovalDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateApprovalDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class EditApprovalDto
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
