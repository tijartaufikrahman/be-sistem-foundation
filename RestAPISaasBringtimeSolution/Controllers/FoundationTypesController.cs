using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Models.DTOs;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoundationTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FoundationTypesController> _logger;

        public FoundationTypesController(AppDbContext context, ILogger<FoundationTypesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _context.FoundationTypes.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationTypesController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _context.FoundationTypes
                    .OrderBy(ft => ft.Id)
                    .Select(ft => new FoundationTypeDto
                    {
                        Id = ft.Id,
                        Name = ft.Name,
                        Code = ft.Code,
                        Description = ft.Description,
                        IsActive = ft.IsActive,
                        FoundationsCount = _context.Foundations.Count(f => f.FoundationTypeId == ft.Id),
                        MenusCount = _context.Menus.Count(m => m.FoundationTypeId == ft.Id),
                        Created_at = ft.Created_at
                    })
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationTypesController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var ft = await _context.FoundationTypes.FindAsync(id);
                if (ft == null) return NotFound(new { success = false, message = "Tipe mitra tidak ditemukan" });

                var dto = new FoundationTypeDto
                {
                    Id = ft.Id,
                    Name = ft.Name,
                    Code = ft.Code,
                    Description = ft.Description,
                    IsActive = ft.IsActive,
                    FoundationsCount = await _context.Foundations.CountAsync(f => f.FoundationTypeId == ft.Id),
                    MenusCount = await _context.Menus.CountAsync(m => m.FoundationTypeId == ft.Id),
                    Created_at = ft.Created_at
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationTypesController.GetById");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFoundationTypeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var normalizedCode = dto.Code.Trim().ToLower().Replace(" ", "_");
                var exists = await _context.FoundationTypes.AnyAsync(ft => ft.Code.ToLower() == normalizedCode);
                if (exists)
                {
                    return BadRequest(new { success = false, message = $"Kode tipe '{dto.Code}' sudah digunakan." });
                }

                var entity = new FoundationType
                {
                    Name = dto.Name.Trim(),
                    Code = normalizedCode,
                    Description = dto.Description?.Trim(),
                    IsActive = dto.IsActive,
                    Created_at = DateTime.UtcNow
                };

                _context.FoundationTypes.Add(entity);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationTypesController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateFoundationTypeDto dto)
        {
            if (id != dto.Id) return BadRequest(new { success = false, message = "ID tidak cocok" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entity = await _context.FoundationTypes.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Tipe mitra tidak ditemukan" });

                var normalizedCode = dto.Code.Trim().ToLower().Replace(" ", "_");
                var exists = await _context.FoundationTypes.AnyAsync(ft => ft.Id != id && ft.Code.ToLower() == normalizedCode);
                if (exists)
                {
                    return BadRequest(new { success = false, message = $"Kode tipe '{dto.Code}' sudah digunakan." });
                }

                entity.Name = dto.Name.Trim();
                entity.Code = normalizedCode;
                entity.Description = dto.Description?.Trim();
                entity.IsActive = dto.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationTypesController.Update");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var entity = await _context.FoundationTypes.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Tipe mitra tidak ditemukan" });

                // Check if any foundations use this type
                var inUseFoundations = await _context.Foundations.AnyAsync(f => f.FoundationTypeId == id);
                if (inUseFoundations)
                {
                    return BadRequest(new { success = false, message = "Tipe mitra tidak dapat dihapus karena masih digunakan oleh entitas mitra/yayasan." });
                }

                // Check if any menus use this type
                var inUseMenus = await _context.Menus.AnyAsync(m => m.FoundationTypeId == id);
                if (inUseMenus)
                {
                    return BadRequest(new { success = false, message = "Tipe mitra tidak dapat dihapus karena masih digunakan oleh menu navigasi." });
                }

                _context.FoundationTypes.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Tipe mitra berhasil dihapus." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationTypesController.Delete");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(long id)
        {
            try
            {
                var entity = await _context.FoundationTypes.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Tipe mitra tidak ditemukan" });

                entity.IsActive = !entity.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(new { success = true, isActive = entity.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationTypesController.ToggleStatus");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
