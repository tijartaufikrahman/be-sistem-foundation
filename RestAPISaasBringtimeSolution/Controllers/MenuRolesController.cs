using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuRolesController : ControllerBase
    {
        private readonly IMenuRoleRepository _menuRoleRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<MenuRolesController> _logger;

        public MenuRolesController(IMenuRoleRepository menuRoleRepo, AppDbContext context, ILogger<MenuRolesController> logger)
        {
            _menuRoleRepo = menuRoleRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _menuRoleRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRolesController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var menuRoles = await (from mr in _context.MenuRoles
                                       join r in _context.Roles on mr.RoleId equals r.Id into roles
                                       from r in roles.DefaultIfEmpty()
                                       join m in _context.Menus on mr.MenuId equals m.Id into menus
                                       from m in menus.DefaultIfEmpty()
                                       join g in _context.Groups on mr.GroupId equals g.Id into groups
                                       from g in groups.DefaultIfEmpty()
                                       orderby mr.RoleId, mr.OrderIndex
                                       select new
                                       {
                                           mr.Id,
                                           mr.RoleId,
                                           RoleName = r != null ? (r.DisplayName ?? r.Name) : "",
                                           mr.MenuId,
                                           MenuLabel = m != null ? m.Label : null,
                                           MenuUrl = m != null ? m.RouteUrl : null,
                                           MenuIcon = m != null ? m.IconClass : null,
                                           MenuNote = m != null ? m.Note : null,
                                           mr.GroupId,
                                           GroupName = g != null ? g.Name : null,
                                           GroupIcon = g != null ? g.IconClass : null,
                                           mr.OrderIndex,
                                           mr.IsActive
                                       }).ToListAsync();

                var groupIds = menuRoles.Where(mr => mr.GroupId.HasValue).Select(mr => mr.GroupId!.Value).Distinct().ToList();

                var groupSubMenus = await (from mg in _context.MenuGroups
                                           where groupIds.Contains(mg.GroupId) && mg.IsActive
                                           join m in _context.Menus on mg.MenuId equals m.Id
                                           orderby mg.OrderIndex
                                           select new
                                           {
                                               mg.GroupId,
                                               SubMenu = new GroupSubMenuDto
                                               {
                                                   MenuId = mg.MenuId,
                                                   Label = m.Label ?? "",
                                                   Url = m.RouteUrl ?? "",
                                                   Icon = m.IconClass,
                                                   OrderIndex = mg.OrderIndex
                                               }
                                           }).ToListAsync();

                var subMenuLookup = groupSubMenus.GroupBy(x => x.GroupId).ToDictionary(g => g.Key, g => g.Select(x => x.SubMenu).ToList());

                var list = menuRoles.Select(mr =>
                {
                    var isGroup = mr.GroupId.HasValue && !string.IsNullOrEmpty(mr.GroupName);
                    var subMenus = (mr.GroupId.HasValue && subMenuLookup.ContainsKey(mr.GroupId.Value))
                        ? subMenuLookup[mr.GroupId.Value]
                        : new List<GroupSubMenuDto>();

                    return new MenuRoleDto
                    {
                        Id = mr.Id,
                        TargetType = isGroup ? "group" : "menu",
                        MenuId = mr.MenuId,
                        MenuLabel = mr.MenuLabel,
                        MenuUrl = mr.MenuUrl,
                        MenuIcon = mr.MenuIcon,
                        MenuNote = mr.MenuNote,
                        GroupId = mr.GroupId,
                        GroupName = mr.GroupName,
                        GroupIcon = mr.GroupIcon,
                        RoleId = mr.RoleId,
                        RoleName = mr.RoleName,
                        OrderIndex = mr.OrderIndex,
                        IsActive = mr.IsActive,
                        SubMenus = subMenus
                    };
                });
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRolesController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRole([FromRoute] long roleId)
        {
            try
            {
                var menuRoles = await (from mr in _context.MenuRoles
                                       where mr.RoleId == roleId && mr.IsActive
                                       join m in _context.Menus on mr.MenuId equals m.Id into menus
                                       from m in menus.DefaultIfEmpty()
                                       join g in _context.Groups on mr.GroupId equals g.Id into groups
                                       from g in groups.DefaultIfEmpty()
                                       orderby mr.OrderIndex
                                       select new
                                       {
                                           mr.Id,
                                           mr.RoleId,
                                           mr.MenuId,
                                           MenuLabel = m != null ? m.Label : null,
                                           MenuUrl = m != null ? m.RouteUrl : null,
                                           MenuIcon = m != null ? m.IconClass : null,
                                           MenuOrderIndex = m != null ? m.OrderIndex : 0,
                                           MenuIsActive = m != null && m.IsActive,
                                           mr.GroupId,
                                           GroupName = g != null ? g.Name : null,
                                           GroupIcon = g != null ? g.IconClass : null,
                                           GroupOrderIndex = g != null ? g.OrderIndex : 0,
                                           GroupIsOpenDefault = g != null && g.IsOpenDefault,
                                           GroupIsActive = g != null && g.IsActive,
                                           mr.OrderIndex,
                                           mr.IsActive
                                       }).ToListAsync();

                var groupIds = menuRoles.Where(mr => mr.GroupId.HasValue && mr.GroupIsActive).Select(mr => mr.GroupId!.Value).Distinct().ToList();

                var groupSubMenus = await (from mg in _context.MenuGroups
                                           where groupIds.Contains(mg.GroupId) && mg.IsActive
                                           join m in _context.Menus on mg.MenuId equals m.Id
                                           where m.IsActive
                                           orderby mg.OrderIndex
                                           select new
                                           {
                                               mg.GroupId,
                                               SubMenu = new
                                               {
                                                   id = mg.Id,
                                                   menuId = mg.MenuId,
                                                   label = m.Label,
                                                   icon = m.IconClass ?? "fas fa-circle",
                                                   url = m.RouteUrl,
                                                   orderIndex = mg.OrderIndex
                                               }
                                           }).ToListAsync();

                var subMenuLookup = groupSubMenus.GroupBy(x => x.GroupId).ToDictionary(g => g.Key, g => g.Select(x => (object)x.SubMenu).ToList());

                var result = new List<object>();

                foreach (var mr in menuRoles)
                {
                    if (mr.GroupId.HasValue && mr.GroupIsActive)
                    {
                        var subMenus = subMenuLookup.ContainsKey(mr.GroupId.Value) ? subMenuLookup[mr.GroupId.Value] : new List<object>();
                        result.Add(new
                        {
                            id = mr.Id,
                            targetType = "group",
                            groupId = mr.GroupId.Value,
                            label = mr.GroupName,
                            icon = mr.GroupIcon ?? "fas fa-folder",
                            url = "",
                            orderIndex = mr.OrderIndex > 0 ? mr.OrderIndex : mr.GroupOrderIndex,
                            isOpenDefault = mr.GroupIsOpenDefault,
                            subMenus = subMenus
                        });
                    }
                    else if (mr.MenuId.HasValue && mr.MenuIsActive)
                    {
                        result.Add(new
                        {
                            id = mr.Id,
                            targetType = "menu",
                            menuId = mr.MenuId.Value,
                            label = mr.MenuLabel,
                            icon = mr.MenuIcon ?? "fas fa-circle",
                            url = mr.MenuUrl,
                            orderIndex = mr.OrderIndex > 0 ? mr.OrderIndex : mr.MenuOrderIndex,
                            subMenus = new List<object>()
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRolesController.GetByRole for RoleId {RoleId}", roleId);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var mr = await (from r in _context.MenuRoles
                                where r.Id == id
                                join ro in _context.Roles on r.RoleId equals ro.Id into roles
                                from ro in roles.DefaultIfEmpty()
                                join m in _context.Menus on r.MenuId equals m.Id into menus
                                from m in menus.DefaultIfEmpty()
                                join g in _context.Groups on r.GroupId equals g.Id into groups
                                from g in groups.DefaultIfEmpty()
                                select new
                                {
                                    r.Id,
                                    r.RoleId,
                                    RoleName = ro != null ? (ro.DisplayName ?? ro.Name) : "",
                                    r.MenuId,
                                    MenuLabel = m != null ? m.Label : null,
                                    MenuUrl = m != null ? m.RouteUrl : null,
                                    MenuIcon = m != null ? m.IconClass : null,
                                    MenuNote = m != null ? m.Note : null,
                                    r.GroupId,
                                    GroupName = g != null ? g.Name : null,
                                    GroupIcon = g != null ? g.IconClass : null,
                                    r.OrderIndex,
                                    r.IsActive
                                }).FirstOrDefaultAsync();

                if (mr == null) return NotFound(new { success = false, message = "MenuRole not found" });

                var isGroup = mr.GroupId.HasValue && !string.IsNullOrEmpty(mr.GroupName);
                return Ok(new MenuRoleDto
                {
                    Id = mr.Id,
                    TargetType = isGroup ? "group" : "menu",
                    MenuId = mr.MenuId,
                    MenuLabel = mr.MenuLabel,
                    MenuUrl = mr.MenuUrl,
                    MenuIcon = mr.MenuIcon,
                    MenuNote = mr.MenuNote,
                    GroupId = mr.GroupId,
                    GroupName = mr.GroupName,
                    GroupIcon = mr.GroupIcon,
                    RoleId = mr.RoleId,
                    RoleName = mr.RoleName,
                    OrderIndex = mr.OrderIndex,
                    IsActive = mr.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRolesController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMenuRoleDto model)
        {
            try
            {
                if (model.RoleId <= 0)
                    return BadRequest(new { success = false, message = "Role wajib dipilih" });

                var isGroup = model.TargetType?.ToLower() == "group" || (model.GroupId.HasValue && model.GroupId > 0 && !model.MenuId.HasValue);

                if (isGroup && (!model.GroupId.HasValue || model.GroupId <= 0))
                    return BadRequest(new { success = false, message = "Grup menu wajib dipilih" });

                if (!isGroup && (!model.MenuId.HasValue || model.MenuId <= 0))
                    return BadRequest(new { success = false, message = "Menu wajib dipilih" });

                var entity = new RestAPISaasBringtimeSolution.Models.Entities.MenuRole
                {
                    RoleId = model.RoleId,
                    MenuId = isGroup ? null : model.MenuId,
                    GroupId = isGroup ? model.GroupId : null,
                    OrderIndex = model.OrderIndex,
                    IsActive = model.IsActive,
                    Created_at = DateTime.UtcNow
                };

                await _menuRoleRepo.AddAsync(entity);
                return Ok(new { success = true, message = "Hak akses menu/grup berhasil ditambahkan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRolesController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id, [FromForm] EditMenuRoleDto model)
        {
            try
            {
                var targetId = id ?? model.Id;
                var entity = await _menuRoleRepo.GetByIdAsync(targetId);
                if (entity == null) return NotFound(new { success = false, message = "MenuRole not found" });

                var isGroup = model.TargetType?.ToLower() == "group" || (model.GroupId.HasValue && model.GroupId > 0 && !model.MenuId.HasValue);

                if (model.RoleId > 0) entity.RoleId = model.RoleId;
                entity.MenuId = isGroup ? null : model.MenuId;
                entity.GroupId = isGroup ? model.GroupId : null;
                entity.OrderIndex = model.OrderIndex;
                entity.IsActive = model.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                await _menuRoleRepo.UpdateAsync(entity);
                return Ok(new { success = true, message = "Hak akses menu/grup berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRolesController.Edit for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long? id, [FromForm] DeleteRequestDto? formDto)
        {
            try
            {
                var targetId = id ?? formDto?.Id;
                if (targetId == null) return BadRequest(new { success = false, message = "Id is required" });
                await _menuRoleRepo.DeleteAsync(targetId.Value);
                return Ok(new { success = true, message = "Menu role berhasil dihapus" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRolesController.Delete for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
