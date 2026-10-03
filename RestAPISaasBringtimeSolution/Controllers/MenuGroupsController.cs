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
    public class MenuGroupsController : ControllerBase
    {
        private readonly IMenuGroupRepository _menuGroupRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<MenuGroupsController> _logger;

        public MenuGroupsController(IMenuGroupRepository menuGroupRepo, AppDbContext context, ILogger<MenuGroupsController> logger)
        {
            _menuGroupRepo = menuGroupRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (from mg in _context.MenuGroups
                                  join g in _context.Groups on mg.GroupId equals g.Id into groups
                                  from g in groups.DefaultIfEmpty()
                                  join m in _context.Menus on mg.MenuId equals m.Id into menus
                                  from m in menus.DefaultIfEmpty()
                                  orderby mg.GroupId, mg.OrderIndex
                                  select new MenuGroupItemDto
                                  {
                                      Id = mg.Id,
                                      GroupId = mg.GroupId,
                                      GroupName = g != null ? g.Name : "",
                                      MenuId = mg.MenuId,
                                      MenuLabel = m != null ? m.Label : "",
                                      MenuUrl = m != null ? m.RouteUrl : "",
                                      MenuIcon = m != null ? m.IconClass : null,
                                      OrderIndex = mg.OrderIndex,
                                      IsActive = mg.IsActive
                                  }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId([FromRoute] long groupId)
        {
            try
            {
                var list = await (from mg in _context.MenuGroups
                                  where mg.GroupId == groupId && mg.IsActive
                                  join g in _context.Groups on mg.GroupId equals g.Id into groups
                                  from g in groups.DefaultIfEmpty()
                                  join m in _context.Menus on mg.MenuId equals m.Id into menus
                                  from m in menus.DefaultIfEmpty()
                                  orderby mg.OrderIndex
                                  select new MenuGroupItemDto
                                  {
                                      Id = mg.Id,
                                      GroupId = mg.GroupId,
                                      GroupName = g != null ? g.Name : "",
                                      MenuId = mg.MenuId,
                                      MenuLabel = m != null ? m.Label : "",
                                      MenuUrl = m != null ? m.RouteUrl : "",
                                      MenuIcon = m != null ? m.IconClass : null,
                                      OrderIndex = mg.OrderIndex,
                                      IsActive = mg.IsActive
                                  }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupsController.GetByGroupId for GroupId {GroupId}", groupId);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var item = await (from mg in _context.MenuGroups
                                  where mg.Id == id
                                  join g in _context.Groups on mg.GroupId equals g.Id into groups
                                  from g in groups.DefaultIfEmpty()
                                  join m in _context.Menus on mg.MenuId equals m.Id into menus
                                  from m in menus.DefaultIfEmpty()
                                  select new MenuGroupItemDto
                                  {
                                      Id = mg.Id,
                                      GroupId = mg.GroupId,
                                      GroupName = g != null ? g.Name : "",
                                      MenuId = mg.MenuId,
                                      MenuLabel = m != null ? m.Label : "",
                                      MenuUrl = m != null ? m.RouteUrl : "",
                                      MenuIcon = m != null ? m.IconClass : null,
                                      OrderIndex = mg.OrderIndex,
                                      IsActive = mg.IsActive
                                  }).FirstOrDefaultAsync();

                if (item == null) return NotFound(new { success = false, message = "Item grup menu tidak ditemukan" });
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupsController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMenuGroupDto model)
        {
            try
            {
                if (model.GroupId <= 0 || model.MenuId <= 0)
                    return BadRequest(new { success = false, message = "Grup dan Menu wajib dipilih" });

                var entity = new MenuGroup
                {
                    GroupId = model.GroupId,
                    MenuId = model.MenuId,
                    OrderIndex = model.OrderIndex,
                    IsActive = model.IsActive,
                    Created_at = DateTime.UtcNow
                };

                await _menuGroupRepo.AddAsync(entity);
                return Ok(new { success = true, message = "Menu berhasil dimasukkan ke grup" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupsController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long id, [FromForm] EditMenuGroupDto model)
        {
            try
            {
                var entity = await _menuGroupRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Item grup menu tidak ditemukan" });

                if (model.GroupId > 0) entity.GroupId = model.GroupId;
                if (model.MenuId > 0) entity.MenuId = model.MenuId;
                entity.OrderIndex = model.OrderIndex;
                entity.IsActive = model.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                await _menuGroupRepo.UpdateAsync(entity);
                return Ok(new { success = true, message = "Item grup menu berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupsController.Edit for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPut("{id}/order")]
        [HttpPost("{id}/order")]
        public async Task<IActionResult> UpdateOrder([FromRoute] long id, [FromQuery] int orderIndex)
        {
            try
            {
                var entity = await _menuGroupRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Item grup menu tidak ditemukan" });

                entity.OrderIndex = orderIndex;
                entity.Updated_at = DateTime.UtcNow;

                await _menuGroupRepo.UpdateAsync(entity);
                return Ok(new { success = true, message = "Urutan berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupsController.UpdateOrder for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                var entity = await _menuGroupRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Item grup menu tidak ditemukan" });

                await _menuGroupRepo.DeleteAsync(id);
                return Ok(new { success = true, message = "Menu berhasil dihapus dari grup" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupsController.Delete for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
