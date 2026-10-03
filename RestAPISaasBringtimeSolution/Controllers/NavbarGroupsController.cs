using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Models.DTOs;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NavbarGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NavbarGroupsController> _logger;

        public NavbarGroupsController(AppDbContext context, ILogger<NavbarGroupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var groups = await _context.NavbarGroups.OrderBy(g => g.Id).ToListAsync();
                var groupIds = groups.Select(g => g.Id).ToList();

                var navbars = await _context.Navbars
                    .Where(n => n.NavbarGroupId.HasValue && groupIds.Contains(n.NavbarGroupId.Value))
                    .OrderBy(n => n.OrderIndex)
                    .ToListAsync();

                var navbarsGrouped = navbars.GroupBy(n => n.NavbarGroupId!.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var result = groups.Select(g => new NavbarGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IconClass = g.IconClass,
                    IsActive = g.IsActive,
                    ChildCount = navbarsGrouped.ContainsKey(g.Id) ? navbarsGrouped[g.Id].Count : 0,
                    Navbars = navbarsGrouped.ContainsKey(g.Id)
                        ? navbarsGrouped[g.Id].Select(n => new NavbarDto
                        {
                            Id = n.Id,
                            NavbarGroupId = n.NavbarGroupId,
                            GroupName = g.Name,
                            Title = n.Title,
                            Url = n.Url,
                            IconClass = n.IconClass,
                            Target = n.Target,
                            Platform = n.Platform,
                            OrderIndex = n.OrderIndex,
                            IsActive = n.IsActive
                        }).ToList()
                        : new List<NavbarDto>()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarGroupsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var g = await _context.NavbarGroups.FindAsync(id);
                if (g == null) return NotFound(new { success = false, message = "Grup navbar tidak ditemukan" });

                var childs = await _context.Navbars
                    .Where(n => n.NavbarGroupId == id)
                    .OrderBy(n => n.OrderIndex)
                    .Select(n => new NavbarDto
                    {
                        Id = n.Id,
                        NavbarGroupId = n.NavbarGroupId,
                        GroupName = g.Name,
                        Title = n.Title,
                        Url = n.Url,
                        IconClass = n.IconClass,
                        Target = n.Target,
                        Platform = n.Platform,
                        OrderIndex = n.OrderIndex,
                        IsActive = n.IsActive
                    }).ToListAsync();

                var dto = new NavbarGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IconClass = g.IconClass,
                    IsActive = g.IsActive,
                    ChildCount = childs.Count,
                    Navbars = childs
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarGroupsController.GetById");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNavbarGroupDto model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Name))
                {
                    return BadRequest(new { success = false, message = "Nama grup navbar wajib diisi" });
                }

                var entity = new NavbarGroup
                {
                    Name = model.Name.Trim(),
                    IconClass = model.IconClass?.Trim(),
                    IsActive = model.IsActive,
                    Created_at = DateTime.UtcNow
                };

                await _context.NavbarGroups.AddAsync(entity);
                await _context.SaveChangesAsync();

                if (model.NavbarIds != null && model.NavbarIds.Count > 0)
                {
                    var selectedNavbars = await _context.Navbars
                        .Where(n => model.NavbarIds.Contains(n.Id))
                        .ToListAsync();
                    foreach (var n in selectedNavbars)
                    {
                        n.NavbarGroupId = entity.Id;
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Grup navbar berhasil dibuat", data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarGroupsController.Create");
                return StatusCode(500, new { success = false, message = "Gagal membuat grup navbar: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] EditNavbarGroupDto model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Name))
                {
                    return BadRequest(new { success = false, message = "Nama grup navbar wajib diisi" });
                }

                var entity = await _context.NavbarGroups.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Grup navbar tidak ditemukan" });

                entity.Name = model.Name.Trim();
                entity.IconClass = model.IconClass?.Trim();
                entity.IsActive = model.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                if (model.NavbarIds != null)
                {
                    // Existing navbars belonging to this group
                    var currentNavbars = await _context.Navbars
                        .Where(n => n.NavbarGroupId == id)
                        .ToListAsync();

                    // Detach navbars not in the new selection
                    foreach (var n in currentNavbars)
                    {
                        if (!model.NavbarIds.Contains(n.Id))
                        {
                            n.NavbarGroupId = null;
                        }
                    }

                    // Attach newly selected navbars
                    var newNavbars = await _context.Navbars
                        .Where(n => model.NavbarIds.Contains(n.Id))
                        .ToListAsync();

                    foreach (var n in newNavbars)
                    {
                        n.NavbarGroupId = id;
                    }
                }

                _context.NavbarGroups.Update(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Grup navbar berhasil diperbarui", data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarGroupsController.Update");
                return StatusCode(500, new { success = false, message = "Gagal memperbarui grup navbar: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                var entity = await _context.NavbarGroups.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Grup navbar tidak ditemukan" });

                // Nullify foreign keys on child navbars
                var childNavbars = await _context.Navbars.Where(n => n.NavbarGroupId == id).ToListAsync();
                foreach (var child in childNavbars)
                {
                    child.NavbarGroupId = null;
                }

                // Delete associated foundation_navbars entries
                var fNavbars = await _context.FoundationNavbars.Where(fn => fn.NavbarGroupId == id).ToListAsync();
                _context.FoundationNavbars.RemoveRange(fNavbars);

                _context.NavbarGroups.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Grup navbar berhasil dihapus" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NavbarGroupsController.Delete");
                return StatusCode(500, new { success = false, message = "Gagal menghapus grup navbar: " + ex.Message });
            }
        }
    }
}
