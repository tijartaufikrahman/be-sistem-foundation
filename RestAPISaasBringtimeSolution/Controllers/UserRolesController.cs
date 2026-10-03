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
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(
            IUserRoleRepository userRoleRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            AppDbContext context,
            ILogger<UserRolesController> logger)
        {
            _userRoleRepo = userRoleRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var dtos = await (from ur in _context.UserRoles
                                  join u in _context.Users on ur.UserId equals u.Id into users
                                  from u in users.DefaultIfEmpty()
                                  join r in _context.Roles on ur.RoleId equals r.Id into roles
                                  from r in roles.DefaultIfEmpty()
                                  select new UserRoleDto
                                  {
                                      Id = ur.Id,
                                      UserId = ur.UserId,
                                      UserName = u != null ? u.Name : $"User #{ur.UserId}",
                                      UserEmail = u != null ? u.Email : "",
                                      RoleId = ur.RoleId,
                                      RoleName = r != null ? r.Name : $"Role #{ur.RoleId}",
                                      RoleDisplayName = r != null ? (r.DisplayName ?? r.Name) : $"Role #{ur.RoleId}",
                                      Created_at = ur.Created_at
                                  }).ToListAsync();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil data penugasan peran");
                return StatusCode(500, new { success = false, message = "Gagal mengambil data penugasan: " + ex.Message });
            }
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUser([FromRoute] long userId)
        {
            try
            {
                var dtos = await (from ur in _context.UserRoles
                                  where ur.UserId == userId
                                  join u in _context.Users on ur.UserId equals u.Id into users
                                  from u in users.DefaultIfEmpty()
                                  join r in _context.Roles on ur.RoleId equals r.Id into roles
                                  from r in roles.DefaultIfEmpty()
                                  select new UserRoleDto
                                  {
                                      Id = ur.Id,
                                      UserId = ur.UserId,
                                      UserName = u != null ? u.Name : $"User #{ur.UserId}",
                                      UserEmail = u != null ? u.Email : "",
                                      RoleId = ur.RoleId,
                                      RoleName = r != null ? r.Name : $"Role #{ur.RoleId}",
                                      RoleDisplayName = r != null ? (r.DisplayName ?? r.Name) : $"Role #{ur.RoleId}",
                                      Created_at = ur.Created_at
                                  }).ToListAsync();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil data peran pengguna");
                return StatusCode(500, new { success = false, message = "Gagal mengambil data peran pengguna: " + ex.Message });
            }
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateJson([FromBody] CreateUserRoleDto model)
        {
            return await HandleCreate(model);
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> CreateForm([FromForm] CreateUserRoleDto model)
        {
            return await HandleCreate(model);
        }

        private async Task<IActionResult> HandleCreate(CreateUserRoleDto model)
        {
            try
            {
                if (model == null || model.UserId <= 0 || model.RoleId <= 0)
                {
                    return BadRequest(new { success = false, message = "Pengguna dan Peran wajib dipilih." });
                }

                var user = await _userRepo.GetByIdAsync(model.UserId);
                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Akun pengguna tidak ditemukan." });
                }

                var role = await _roleRepo.GetByIdAsync(model.RoleId);
                if (role == null)
                {
                    return BadRequest(new { success = false, message = "Peran (Role) tidak ditemukan." });
                }

                // Cek apakah pengguna sudah memiliki peran tersebut
                var existingRoles = await _userRoleRepo.GetByUserIdAsync(model.UserId);
                if (existingRoles.Any(ur => ur.RoleId == model.RoleId))
                {
                    return BadRequest(new { success = false, message = $"Pengguna '{user.Name}' sudah memiliki peran '{role.DisplayName ?? role.Name}'." });
                }

                var entity = new UserRole
                {
                    UserId = model.UserId,
                    RoleId = model.RoleId,
                    Created_at = DateTime.UtcNow
                };

                await _userRoleRepo.AddAsync(entity);
                return Ok(new 
                { 
                    success = true, 
                    id = entity.Id, 
                    message = $"Peran '{role.DisplayName ?? role.Name}' berhasil dipasangkan ke '{user.Name}'." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menugaskan peran");
                return StatusCode(500, new { success = false, message = "Gagal menugaskan peran: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                await _userRoleRepo.DeleteAsync(id);
                return Ok(new { success = true, message = "Penugasan peran berhasil dilepas." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus penugasan peran");
                return StatusCode(500, new { success = false, message = "Gagal melepas peran: " + ex.Message });
            }
        }

        [HttpPost("delete")]
        [Consumes("application/json")]
        public async Task<IActionResult> DeleteJson([FromBody] DeleteRequestDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest(new { success = false, message = "Id penugasan wajib diisi." });
            return await Delete(dto.Id);
        }

        [HttpPost("delete")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> DeleteForm([FromForm] DeleteRequestDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest(new { success = false, message = "Id penugasan wajib diisi." });
            return await Delete(dto.Id);
        }
    }
}
