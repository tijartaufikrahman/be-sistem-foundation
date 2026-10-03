namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class MenuRoleDto
    {
        public long Id { get; set; }
        public string TargetType { get; set; } = "menu"; // "menu" or "group"
        public long? MenuId { get; set; }
        public string? MenuLabel { get; set; }
        public string? MenuUrl { get; set; }
        public string? MenuIcon { get; set; }
        public string? MenuNote { get; set; }
        
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupIcon { get; set; }
        
        public long RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }

        // Sub-menus if this item is a group
        public List<GroupSubMenuDto> SubMenus { get; set; } = new List<GroupSubMenuDto>();
    }

    public class GroupSubMenuDto
    {
        public long MenuId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int OrderIndex { get; set; }
    }

    public class CreateMenuRoleDto
    {
        public string TargetType { get; set; } = "menu"; // "menu" or "group"
        public long? MenuId { get; set; }
        public long? GroupId { get; set; }
        public long RoleId { get; set; }
        public int OrderIndex { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }

    public class EditMenuRoleDto
    {
        public long Id { get; set; }
        public string TargetType { get; set; } = "menu"; // "menu" or "group"
        public long? MenuId { get; set; }
        public long? GroupId { get; set; }
        public long RoleId { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }
}
