using Microsoft.AspNetCore.Mvc;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepo;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleRepository roleRepo, ILogger<RolesController> logger)
        {
            _roleRepo = roleRepo;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _roleRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RolesController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _roleRepo.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RolesController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var entity = await _roleRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Role tidak ditemukan" });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RolesController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateJson([FromBody] CreateRoleDto model)
        {
            return await HandleCreate(model);
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> CreateForm([FromForm] CreateRoleDto model)
        {
            return await HandleCreate(model);
        }

        private async Task<IActionResult> HandleCreate(CreateRoleDto model)
        {
            try
            {
                if (model == null) return BadRequest(new { success = false, message = "Data peran tidak boleh kosong." });
                if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest(new { success = false, message = "Kode peran wajib diisi." });
                if (string.IsNullOrWhiteSpace(model.DisplayName)) return BadRequest(new { success = false, message = "Nama peran (Display Name) wajib diisi." });

                var cleanName = model.Name.Trim().ToLower().Replace(" ", "_");
                var allRoles = await _roleRepo.GetAllAsync();
                if (allRoles.Any(r => r.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { success = false, message = $"Kode peran '{cleanName}' sudah terdaftar. Silakan gunakan kode lain." });
                }

                var entity = new Role
                {
                    Name = cleanName,
                    DisplayName = model.DisplayName.Trim(),
                    Description = !string.IsNullOrWhiteSpace(model.Description) ? model.Description.Trim() : null,
                    Created_at = DateTime.UtcNow
                };

                await _roleRepo.AddAsync(entity);
                return Ok(new { success = true, id = entity.Id, message = "Peran berhasil ditambahkan." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membuat peran");
                return StatusCode(500, new { success = false, message = "Gagal menyimpan peran: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> EditJson([FromRoute] long id, [FromBody] EditRoleDto model)
        {
            return await HandleEdit(id, model);
        }

        [HttpPost("{id}")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> EditForm([FromRoute] long? id, [FromForm] EditRoleDto model)
        {
            return await HandleEdit(id ?? model.Id, model);
        }

        private async Task<IActionResult> HandleEdit(long targetId, EditRoleDto model)
        {
            try
            {
                var entity = await _roleRepo.GetByIdAsync(targetId);
                if (entity == null) return NotFound(new { success = false, message = "Peran tidak ditemukan" });

                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    var cleanName = model.Name.Trim().ToLower().Replace(" ", "_");
                    var allRoles = await _roleRepo.GetAllAsync();
                    if (allRoles.Any(r => r.Id != targetId && r.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                    {
                        return BadRequest(new { success = false, message = $"Kode peran '{cleanName}' sudah digunakan oleh peran lain." });
                    }
                    entity.Name = cleanName;
                }

                if (!string.IsNullOrWhiteSpace(model.DisplayName)) entity.DisplayName = model.DisplayName.Trim();
                if (model.Description != null) entity.Description = !string.IsNullOrWhiteSpace(model.Description) ? model.Description.Trim() : null;

                entity.Updated_at = DateTime.UtcNow;
                await _roleRepo.UpdateAsync(entity);
                return Ok(new { success = true, message = "Data peran berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memperbarui peran");
                return StatusCode(500, new { success = false, message = "Gagal memperbarui peran: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                if (await _roleRepo.HasUsersAsync(id))
                {
                    return BadRequest(new { success = false, message = "Peran tidak dapat dihapus karena masih ditugaskan kepada beberapa pengguna. Lepaskan penugasan di User Roles terlebih dahulu." });
                }

                await _roleRepo.DeleteAsync(id);
                return Ok(new { success = true, message = "Peran berhasil dihapus." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus peran");
                return StatusCode(500, new { success = false, message = "Gagal menghapus peran: " + ex.Message });
            }
        }

        [HttpPost("delete")]
        [Consumes("application/json")]
        public async Task<IActionResult> DeleteJson([FromBody] DeleteRequestDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest(new { success = false, message = "Id peran wajib diisi." });
            return await Delete(dto.Id);
        }

        [HttpPost("delete")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> DeleteForm([FromForm] DeleteRequestDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest(new { success = false, message = "Id peran wajib diisi." });
            return await Delete(dto.Id);
        }
    }
}
