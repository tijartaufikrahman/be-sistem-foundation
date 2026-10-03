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
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepo, 
            IUserRoleRepository userRoleRepo, 
            IRoleRepository roleRepo,
            AppDbContext context,
            ILogger<UsersController> logger)
        {
            _userRepo = userRepo;
            _userRoleRepo = userRoleRepo;
            _roleRepo = roleRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _userRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UsersController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var userRolesWithRole = await (from ur in _context.UserRoles
                                               join r in _context.Roles on ur.RoleId equals r.Id
                                               select new { ur.UserId, RoleName = r.DisplayName ?? r.Name }).ToListAsync();

                var dtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AvatarUrl = u.AvatarUrl,
                    AuthProvider = u.AuthProvider,
                    IsActive = u.IsActive,
                    Roles = string.Join(", ", userRolesWithRole.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleName))
                });
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UsersController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (u == null) return NotFound(new { success = false, message = "User not found" });

                var userRoles = await (from ur in _context.UserRoles
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       where ur.UserId == id
                                       select (r.DisplayName ?? r.Name)).ToListAsync();

                return Ok(new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AvatarUrl = u.AvatarUrl,
                    AuthProvider = u.AuthProvider,
                    IsActive = u.IsActive,
                    Roles = string.Join(", ", userRoles)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UsersController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateJson([FromBody] CreateUserDto model)
        {
            return await HandleCreate(model);
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> CreateForm([FromForm] CreateUserDto model)
        {
            return await HandleCreate(model);
        }

        private async Task<IActionResult> HandleCreate(CreateUserDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { success = false, message = "Data pengguna tidak boleh kosong." });

                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest(new { success = false, message = "Nama lengkap wajib diisi." });

                if (string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest(new { success = false, message = "Alamat email wajib diisi." });

                if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
                    return BadRequest(new { success = false, message = "Kata sandi (password) minimal 6 karakter." });

                var emailClean = model.Email.Trim().ToLower();
                var existing = await _userRepo.GetByEmailAsync(emailClean);
                if (existing != null)
                {
                    return BadRequest(new { success = false, message = $"Email '{model.Email.Trim()}' sudah terdaftar. Silakan gunakan email lain." });
                }

                Role? assignedRole = null;
                if (model.RoleId.HasValue && model.RoleId.Value > 0)
                {
                    assignedRole = await _roleRepo.GetByIdAsync(model.RoleId.Value);
                    if (assignedRole == null)
                    {
                        return BadRequest(new { success = false, message = "Peran (Role) yang dipilih tidak valid atau tidak ditemukan." });
                    }
                }

                var entity = new RestAPISaasBringtimeSolution.Models.Entities.User
                {
                    Name = model.Name.Trim(),
                    Email = emailClean,
                    PhoneNumber = !string.IsNullOrWhiteSpace(model.PhoneNumber) ? model.PhoneNumber.Trim() : null,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    AuthProvider = "local",
                    IsActive = true,
                    Created_at = DateTime.UtcNow
                };

                await _userRepo.AddAsync(entity);
                await _userRepo.SaveChangesAsync();

                if (assignedRole != null)
                {
                    await _userRoleRepo.AddAsync(new RestAPISaasBringtimeSolution.Models.Entities.UserRole
                    {
                        UserId = entity.Id,
                        RoleId = assignedRole.Id,
                        Created_at = DateTime.UtcNow
                    });
                    await _userRoleRepo.SaveChangesAsync();
                }

                return Ok(new { success = true, id = entity.Id, message = "Pengguna berhasil ditambahkan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membuat pengguna baru");
                return StatusCode(500, new { success = false, message = "Gagal menyimpan pengguna: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditJson([FromRoute] long id, [FromBody] EditUserDto model)
        {
            return await HandleEdit(id, model);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditForm([FromRoute] long? id, [FromForm] EditUserDto model)
        {
            return await HandleEdit(id ?? model.Id, model);
        }

        private async Task<IActionResult> HandleEdit(long targetId, EditUserDto model)
        {
            try
            {
                var entity = await _userRepo.GetByIdAsync(targetId);
                if (entity == null) return NotFound(new { success = false, message = "User tidak ditemukan" });

                if (!string.IsNullOrEmpty(model.Name)) entity.Name = model.Name.Trim();

                if (!string.IsNullOrEmpty(model.Email))
                {
                    var emailClean = model.Email.Trim().ToLower();
                    var existingOther = await _userRepo.GetByEmailAsync(emailClean);
                    if (existingOther != null && existingOther.Id != targetId)
                    {
                        return BadRequest(new { success = false, message = $"Email '{model.Email.Trim()}' sudah digunakan oleh akun lain." });
                    }
                    entity.Email = emailClean;
                }

                if (model.PhoneNumber != null) entity.PhoneNumber = !string.IsNullOrWhiteSpace(model.PhoneNumber) ? model.PhoneNumber.Trim() : null;
                if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;

                if (!string.IsNullOrEmpty(model.Password) && model.Password.Length >= 6)
                {
                    entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                Role? assignedRole = null;
                if (model.RoleId.HasValue && model.RoleId.Value > 0)
                {
                    assignedRole = await _roleRepo.GetByIdAsync(model.RoleId.Value);
                    if (assignedRole == null)
                    {
                        return BadRequest(new { success = false, message = "Peran (Role) yang dipilih tidak valid atau tidak ditemukan." });
                    }
                }

                entity.Updated_at = DateTime.UtcNow;
                await _userRepo.UpdateAsync(entity);
                await _userRepo.SaveChangesAsync();

                if (model.RoleId.HasValue)
                {
                    var existingRoles = await _userRoleRepo.GetByUserIdAsync(targetId);
                    foreach (var ur in existingRoles)
                    {
                        await _userRoleRepo.DeleteAsync(ur.Id);
                    }

                    if (assignedRole != null)
                    {
                        await _userRoleRepo.AddAsync(new RestAPISaasBringtimeSolution.Models.Entities.UserRole
                        {
                            UserId = targetId,
                            RoleId = assignedRole.Id,
                            Created_at = DateTime.UtcNow
                        });
                    }
                    await _userRoleRepo.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "User berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengedit pengguna");
                return StatusCode(500, new { success = false, message = "Gagal memperbarui pengguna: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                var u = await _userRepo.GetByIdAsync(id);
                if (u == null) return NotFound(new { success = false, message = "Pengguna tidak ditemukan." });

                var uRoles = await _userRoleRepo.GetByUserIdAsync(id);
                foreach (var ur in uRoles)
                {
                    await _userRoleRepo.DeleteAsync(ur.Id);
                }
                await _userRoleRepo.SaveChangesAsync();

                await _userRepo.DeleteAsync(id);
                await _userRepo.SaveChangesAsync();
                return Ok(new { success = true, message = "Pengguna berhasil dihapus." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus pengguna");
                return StatusCode(500, new { success = false, message = "Gagal menghapus pengguna: " + ex.Message });
            }
        }

        [HttpPost("delete")]
        [Consumes("application/json")]
        public async Task<IActionResult> DeleteJson([FromBody] DeleteRequestDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest(new { success = false, message = "Id pengguna wajib diisi." });
            return await Delete(dto.Id);
        }

        [HttpPost("delete")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> DeleteForm([FromForm] DeleteRequestDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest(new { success = false, message = "Id pengguna wajib diisi." });
            return await Delete(dto.Id);
        }
    }
}
