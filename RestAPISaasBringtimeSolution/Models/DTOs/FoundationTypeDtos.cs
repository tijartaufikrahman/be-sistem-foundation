using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class FoundationTypeDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int FoundationsCount { get; set; }
        public int MenusCount { get; set; }
        public DateTime? Created_at { get; set; }
    }

    public class CreateFoundationTypeDto
    {
        [Required(ErrorMessage = "Nama tipe mitra wajib diisi")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kode unik tipe mitra wajib diisi")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateFoundationTypeDto
    {
        [Required]
        public long Id { get; set; }

        [Required(ErrorMessage = "Nama tipe mitra wajib diisi")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kode unik tipe mitra wajib diisi")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
