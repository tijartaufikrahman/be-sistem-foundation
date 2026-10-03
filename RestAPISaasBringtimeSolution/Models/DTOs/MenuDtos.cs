using System.ComponentModel.DataAnnotations;

namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class MenuDto
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public long? FoundationTypeId { get; set; }
        public string? FoundationTypeName { get; set; }
        public string Label { get; set; } = null!;
        public string RouteUrl { get; set; } = null!;
        public string? IconClass { get; set; }
        public int OrderIndex { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateMenuDto
    {
        public long? ParentId { get; set; }
        public long? FoundationTypeId { get; set; }
        [Required]
        public string Label { get; set; } = null!;
        [Required]
        public string RouteUrl { get; set; } = null!;
        public string? IconClass { get; set; }
        public int OrderIndex { get; set; }
        public string? Note { get; set; }
    }

    public class EditMenuDto
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public long? FoundationTypeId { get; set; }
        [Required]
        public string Label { get; set; } = null!;
        [Required]
        public string RouteUrl { get; set; } = null!;
        public string? IconClass { get; set; }
        public int OrderIndex { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
    }
}
