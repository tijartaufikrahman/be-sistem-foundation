namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class MenuGroupItemDto
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public long MenuId { get; set; }
        public string MenuLabel { get; set; } = string.Empty;
        public string MenuUrl { get; set; } = string.Empty;
        public string? MenuIcon { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateMenuGroupDto
    {
        public long GroupId { get; set; }
        public long MenuId { get; set; }
        public int OrderIndex { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class EditMenuGroupDto
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public long MenuId { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }
}
