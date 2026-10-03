namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class GroupDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int OrderIndex { get; set; }
        public string? Note { get; set; }
        public bool IsOpenDefault { get; set; }
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }
        public List<MenuGroupItemDto> Items { get; set; } = new List<MenuGroupItemDto>();
    }

    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int OrderIndex { get; set; } = 0;
        public string? Note { get; set; }
        public bool IsOpenDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class EditGroupDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int OrderIndex { get; set; }
        public string? Note { get; set; }
        public bool IsOpenDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
