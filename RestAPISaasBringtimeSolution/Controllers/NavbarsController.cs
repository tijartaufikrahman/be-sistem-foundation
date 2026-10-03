using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Models.DTOs;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NavbarsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NavbarsController> _logger;

        public NavbarsController(AppDbContext context, ILogger<NavbarsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] long? groupId, [FromQuery] bool? standaloneOnly)
        {
            try
            {
                var query = _context.Navbars.AsQueryable();

                if (groupId.HasValue)
                {
                    query = query.Where(n => n.NavbarGroupId == groupId.Value);
                }
                else if (standaloneOnly == true)
                {
                    query = query.Where(n => n.NavbarGroupId == null);
                }

                var navbars = await query.OrderBy(n => n.OrderIndex).ThenBy(n => n.Id).ToListAsync();
                var groupIds = navbars.Where(n => n.NavbarGroupId.HasValue).Select(n => n.NavbarGroupId!.Value).Distinct().ToList();

                var groupsDict = await _context.NavbarGroups
                    .Where(g => groupIds.Contains(g.Id))
                    .ToDictionaryAsync(g => g.Id, g => g.Name);

                var result = navbars.Select(n => new NavbarDto
                {
                    Id = n.Id,
                    NavbarGroupId = n.NavbarGroupId,
                    GroupName = n.NavbarGroupId.HasValue && groupsDict.ContainsKey(n.NavbarGroupId.Value) ? groupsDict[n.NavbarGroupId.Value] : null,
                    Title = n.Title,
                    Url = FoundationNavbarsController.CleanNavbarUrl(n.Url),
                    IconClass = n.IconClass,
                    Target = n.Target ?? "_self",
                    Platform = n.Platform ?? "all",
                    OrderIndex = n.OrderIndex,
                    IsActive = n.IsActive
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var n = await _context.Navbars.FindAsync(id);
                if (n == null) return NotFound(new { success = false, message = "Butir navbar tidak ditemukan" });

                string? groupName = null;
                if (n.NavbarGroupId.HasValue)
                {
                    var g = await _context.NavbarGroups.FindAsync(n.NavbarGroupId.Value);
                    groupName = g?.Name;
                }

                var dto = new NavbarDto
                {
                    Id = n.Id,
                    NavbarGroupId = n.NavbarGroupId,
                    GroupName = groupName,
                    Title = n.Title,
                    Url = FoundationNavbarsController.CleanNavbarUrl(n.Url),
                    IconClass = n.IconClass,
                    Target = n.Target ?? "_self",
                    Platform = n.Platform ?? "all",
                    OrderIndex = n.OrderIndex,
                    IsActive = n.IsActive
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarsController.GetById");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNavbarDto model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Url))
                {
                    return BadRequest(new { success = false, message = "Title dan URL navbar wajib diisi" });
                }

                var entity = new Navbar
                {
                    NavbarGroupId = model.NavbarGroupId > 0 ? model.NavbarGroupId : null,
                    Title = model.Title.Trim(),
                    Url = FoundationNavbarsController.CleanNavbarUrl(model.Url),
                    IconClass = model.IconClass?.Trim(),
                    Target = string.IsNullOrWhiteSpace(model.Target) ? "_self" : model.Target.Trim(),
                    Platform = string.IsNullOrWhiteSpace(model.Platform) ? "all" : model.Platform.Trim(),
                    OrderIndex = model.OrderIndex,
                    IsActive = model.IsActive,
                    Created_at = DateTime.UtcNow
                };

                await _context.Navbars.AddAsync(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Butir navbar berhasil dibuat", data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarsController.Create");
                return StatusCode(500, new { success = false, message = "Gagal membuat butir navbar: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] EditNavbarDto model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Url))
                {
                    return BadRequest(new { success = false, message = "Title dan URL navbar wajib diisi" });
                }

                var entity = await _context.Navbars.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Butir navbar tidak ditemukan" });

                entity.NavbarGroupId = model.NavbarGroupId > 0 ? model.NavbarGroupId : null;
                entity.Title = model.Title.Trim();
                entity.Url = FoundationNavbarsController.CleanNavbarUrl(model.Url);
                entity.IconClass = model.IconClass?.Trim();
                entity.Target = string.IsNullOrWhiteSpace(model.Target) ? "_self" : model.Target.Trim();
                entity.Platform = string.IsNullOrWhiteSpace(model.Platform) ? "all" : model.Platform.Trim();
                entity.OrderIndex = model.OrderIndex;
                entity.IsActive = model.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                _context.Navbars.Update(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Butir navbar berhasil diperbarui", data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarsController.Update");
                return StatusCode(500, new { success = false, message = "Gagal memperbarui butir navbar: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                var entity = await _context.Navbars.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Butir navbar tidak ditemukan" });

                var fNavbars = await _context.FoundationNavbars.Where(fn => fn.NavbarId == id).ToListAsync();
                _context.FoundationNavbars.RemoveRange(fNavbars);

                _context.Navbars.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Butir navbar berhasil dihapus" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarsController.Delete");
                return StatusCode(500, new { success = false, message = "Gagal menghapus butir navbar: " + ex.Message });
            }
        }
    }
}
