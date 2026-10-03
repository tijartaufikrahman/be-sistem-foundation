using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenusController : ControllerBase
    {
        private readonly IMenuRepository _menuRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<MenusController> _logger;

        public MenusController(IMenuRepository menuRepo, AppDbContext context, ILogger<MenusController> logger)
        {
            _menuRepo = menuRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _menuRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenusController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] long? foundationTypeId)
        {
            try
            {
                var query = from m in _context.Menus
                            join ft in _context.FoundationTypes on m.FoundationTypeId equals ft.Id into fts
                            from ft in fts.DefaultIfEmpty()
                            select new MenuDto
                            {
                                Id = m.Id,
                                ParentId = m.ParentId,
                                FoundationTypeId = m.FoundationTypeId,
                                FoundationTypeName = ft != null ? ft.Name : null,
                                Label = m.Label,
                                RouteUrl = m.RouteUrl,
                                IconClass = m.IconClass,
                                OrderIndex = m.OrderIndex,
                                Note = m.Note,
                                IsActive = m.IsActive
                            };

                if (foundationTypeId.HasValue)
                {
                    query = query.Where(m => m.FoundationTypeId == foundationTypeId.Value || m.FoundationTypeId == null);
                }

                var result = await query.OrderBy(m => m.OrderIndex).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenusController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMenuDto model)
        {
            try
            {
                if (model == null) return BadRequest(new { success = false, message = "Data menu tidak boleh kosong" });
                if (string.IsNullOrWhiteSpace(model.Label)) return BadRequest(new { success = false, message = "Label menu wajib diisi" });
                if (string.IsNullOrWhiteSpace(model.RouteUrl)) return BadRequest(new { success = false, message = "URL Route menu wajib diisi" });

                var entity = new RestAPISaasBringtimeSolution.Models.Entities.Menu
                {
                    ParentId = model.ParentId,
                    FoundationTypeId = model.FoundationTypeId,
                    Label = model.Label.Trim(),
                    RouteUrl = model.RouteUrl.Trim(),
                    IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? "fas fa-circle-dot" : model.IconClass.Trim(),
                    OrderIndex = model.OrderIndex,
                    Note = model.Note?.Trim()
                };
                await _menuRepo.AddAsync(entity);
                return Ok(new { success = true, id = entity.Id, message = "Menu navigasi berhasil ditambahkan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenusController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id, [FromForm] EditMenuDto model)
        {
            try
            {
                var targetId = id ?? model.Id;
                var entity = await _menuRepo.GetByIdAsync(targetId);
                if (entity == null) return NotFound(new { success = false, message = "Menu navigasi tidak ditemukan" });

                if (!string.IsNullOrWhiteSpace(model.Label)) entity.Label = model.Label.Trim();
                if (!string.IsNullOrWhiteSpace(model.RouteUrl)) entity.RouteUrl = model.RouteUrl.Trim();
                if (model.IconClass != null) entity.IconClass = model.IconClass.Trim();
                
                entity.ParentId = model.ParentId;
                entity.FoundationTypeId = model.FoundationTypeId;
                entity.OrderIndex = model.OrderIndex;
                entity.Note = model.Note?.Trim();
                entity.IsActive = model.IsActive;
                
                await _menuRepo.UpdateAsync(entity);
                return Ok(new { success = true, message = "Menu navigasi berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenusController.Edit for Id {Id}", id);
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
                await _menuRepo.DeleteAsync(targetId.Value);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenusController.Delete for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
