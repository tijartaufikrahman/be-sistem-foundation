namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    // ==========================================
    // NAVBAR GROUPS DTOs
    // ==========================================
    public class NavbarGroupDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public bool IsActive { get; set; }
        public int ChildCount { get; set; }
        public List<NavbarDto> Navbars { get; set; } = new();
    }

    public class CreateNavbarGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public bool IsActive { get; set; } = true;
        public List<long>? NavbarIds { get; set; }
    }

    public class EditNavbarGroupDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public bool IsActive { get; set; }
        public List<long>? NavbarIds { get; set; }
    }

    // ==========================================
    // NAVBARS DTOs (Master Butir Menu)
    // ==========================================
    public class NavbarDto
    {
        public long Id { get; set; }
        public long? NavbarGroupId { get; set; }
        public string? GroupName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string Target { get; set; } = "_self";
        public string Platform { get; set; } = "all";
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateNavbarDto
    {
        public long? NavbarGroupId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string Target { get; set; } = "_self";
        public string Platform { get; set; } = "all";
        public int OrderIndex { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class EditNavbarDto
    {
        public long Id { get; set; }
        public long? NavbarGroupId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string Target { get; set; } = "_self";
        public string Platform { get; set; } = "all";
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }

    // ==========================================
    // FOUNDATION NAVBARS DTOs (Assignment per Yayasan)
    // ==========================================
    public class FoundationNavbarDto
    {
        public long Id { get; set; }
        public long FoundationId { get; set; }
        public string? FoundationName { get; set; }
        public string? FoundationSlug { get; set; }

        public long? NavbarId { get; set; }
        public string? NavbarTitle { get; set; }
        public string? NavbarUrl { get; set; }
        public string? NavbarIcon { get; set; }

        public long? NavbarGroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupIcon { get; set; }
        public int ChildCount { get; set; }

        public bool IsDropdown => NavbarGroupId.HasValue && NavbarGroupId.Value > 0;
        public string DisplayLabel => IsDropdown ? (GroupName ?? "Dropdown") : (NavbarTitle ?? "Menu");

        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateFoundationNavbarDto
    {
        public long FoundationId { get; set; }
        public long? NavbarId { get; set; }
        public long? NavbarGroupId { get; set; }
        public int OrderIndex { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class EditFoundationNavbarDto
    {
        public long Id { get; set; }
        public long FoundationId { get; set; }
        public long? NavbarId { get; set; }
        public long? NavbarGroupId { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }

    public class BulkSyncFoundationNavbarItem
    {
        public long? Id { get; set; }
        public long? NavbarId { get; set; }
        public long? NavbarGroupId { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BulkSyncFoundationNavbarDto
    {
        public long FoundationId { get; set; }
        public List<BulkSyncFoundationNavbarItem> Items { get; set; } = new();
    }

    // ==========================================
    // PUBLIC PORTAL RENDER DTO
    // ==========================================
    public class PublicNavbarItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string Target { get; set; } = "_self";
        public bool IsDropdown { get; set; } = false;
        public int OrderIndex { get; set; }
        public List<PublicNavbarItemDto> DropdownItems { get; set; } = new();
    }
}
