using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupRepository _groupRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IGroupRepository groupRepo, AppDbContext context, ILogger<GroupsController> logger)
        {
            _groupRepo = groupRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _groupRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupsController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var groups = (await _groupRepo.GetAllAsync()).ToList();
                var groupIds = groups.Select(g => g.Id).ToList();

                var menuGroupItems = await (from mg in _context.MenuGroups
                                            where groupIds.Contains(mg.GroupId) && mg.IsActive
                                            join m in _context.Menus on mg.MenuId equals m.Id
                                            join g in _context.Groups on mg.GroupId equals g.Id
                                            orderby mg.OrderIndex
                                            select new MenuGroupItemDto
                                            {
                                                Id = mg.Id,
                                                GroupId = mg.GroupId,
                                                GroupName = g.Name,
                                                MenuId = mg.MenuId,
                                                MenuLabel = m.Label ?? "",
                                                MenuUrl = m.RouteUrl ?? "",
                                                MenuIcon = m.IconClass,
                                                OrderIndex = mg.OrderIndex,
                                                IsActive = mg.IsActive
                                            }).ToListAsync();

                var groupedItems = menuGroupItems.GroupBy(x => x.GroupId).ToDictionary(g => g.Key, g => g.ToList());

                var list = groups.Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IconClass = g.IconClass,
                    OrderIndex = g.OrderIndex,
                    Note = g.Note,
                    IsOpenDefault = g.IsOpenDefault,
                    IsActive = g.IsActive,
                    ItemCount = groupedItems.ContainsKey(g.Id) ? groupedItems[g.Id].Count : 0,
                    Items = groupedItems.ContainsKey(g.Id) ? groupedItems[g.Id] : new List<MenuGroupItemDto>()
                });
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var g = await _groupRepo.GetByIdAsync(id);
                if (g == null) return NotFound(new { success = false, message = "Group tidak ditemukan" });

                var items = await (from mg in _context.MenuGroups
                                   where mg.GroupId == id && mg.IsActive
                                   join m in _context.Menus on mg.MenuId equals m.Id
                                   orderby mg.OrderIndex
                                   select new MenuGroupItemDto
                                   {
                                       Id = mg.Id,
                                       GroupId = mg.GroupId,
                                       GroupName = g.Name,
                                       MenuId = mg.MenuId,
                                       MenuLabel = m.Label ?? "",
                                       MenuUrl = m.RouteUrl ?? "",
                                       MenuIcon = m.IconClass,
                                       OrderIndex = mg.OrderIndex,
                                       IsActive = mg.IsActive
                                   }).ToListAsync();

                return Ok(new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IconClass = g.IconClass,
                    OrderIndex = g.OrderIndex,
                    Note = g.Note,
                    IsOpenDefault = g.IsOpenDefault,
                    IsActive = g.IsActive,
                    ItemCount = items.Count,
                    Items = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupsController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateGroupDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest(new { success = false, message = "Nama grup wajib diisi" });

                var entity = new Group
                {
                    Name = model.Name.Trim(),
                    IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? "fas fa-folder" : model.IconClass.Trim(),
                    OrderIndex = model.OrderIndex,
                    Note = model.Note?.Trim(),
                    IsOpenDefault = model.IsOpenDefault,
                    IsActive = model.IsActive,
                    Created_at = DateTime.UtcNow
                };

                await _groupRepo.AddAsync(entity);
                return Ok(new { success = true, message = "Grup menu berhasil ditambahkan", data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupsController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long id, [FromForm] EditGroupDto model)
        {
            try
            {
                var entity = await _groupRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Grup menu tidak ditemukan" });

                if (!string.IsNullOrWhiteSpace(model.Name))
                    entity.Name = model.Name.Trim();

                if (model.IconClass != null)
                    entity.IconClass = model.IconClass.Trim();

                entity.OrderIndex = model.OrderIndex;
                entity.Note = model.Note?.Trim();
                entity.IsOpenDefault = model.IsOpenDefault;
                entity.IsActive = model.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                await _groupRepo.UpdateAsync(entity);
                return Ok(new { success = true, message = "Grup menu berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupsController.Edit for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}/toggle-default-open")]
        [HttpPatch("{id}/toggle-default-open")]
        public async Task<IActionResult> ToggleDefaultOpen([FromRoute] long id, [FromQuery] bool? isOpen)
        {
            try
            {
                var entity = await _groupRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Grup menu tidak ditemukan" });

                entity.IsOpenDefault = isOpen ?? !entity.IsOpenDefault;
                entity.Updated_at = DateTime.UtcNow;
                await _groupRepo.UpdateAsync(entity);

                return Ok(new { success = true, isOpenDefault = entity.IsOpenDefault, message = $"Status default dropdown berhasil diubah menjadi {(entity.IsOpenDefault ? "Terbuka" : "Tertutup")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupsController.ToggleDefaultOpen for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                var entity = await _groupRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Grup menu tidak ditemukan" });

                await _groupRepo.DeleteAsync(id);
                return Ok(new { success = true, message = "Grup menu berhasil dihapus" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupsController.Delete for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
