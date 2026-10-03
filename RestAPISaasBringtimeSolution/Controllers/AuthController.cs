using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthRepository authRepo, AppDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _authRepo = authRepo;
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto? jsonModel)
        {
            try
            {
                string? email = jsonModel?.Email;
                string? password = jsonModel?.Password;
                string? foundationSlug = jsonModel?.FoundationSlug ?? jsonModel?.FoundationDomain;
                long? foundationId = jsonModel?.FoundationId;

                if (string.IsNullOrEmpty(email) && Request.HasFormContentType)
                {
                    email = Request.Form["Email"].FirstOrDefault() ?? Request.Form["email"].FirstOrDefault();
                    password = Request.Form["Password"].FirstOrDefault() ?? Request.Form["password"].FirstOrDefault();
                    foundationSlug = Request.Form["FoundationSlug"].FirstOrDefault() ?? Request.Form["foundationSlug"].FirstOrDefault()
                        ?? Request.Form["FoundationDomain"].FirstOrDefault() ?? Request.Form["foundationDomain"].FirstOrDefault();
                    if (long.TryParse(Request.Form["FoundationId"].FirstOrDefault() ?? Request.Form["foundationId"].FirstOrDefault(), out var parsedFid))
                    {
                        foundationId = parsedFid;
                    }
                }

                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("foundation"))
                {
                    foundationSlug = Request.Query["foundation"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("domain"))
                {
                    foundationSlug = Request.Query["domain"].FirstOrDefault();
                }
                if (!foundationId.HasValue && Request.Query.ContainsKey("foundationId") && long.TryParse(Request.Query["foundationId"].FirstOrDefault(), out var qFid))
                {
                    foundationId = qFid;
                }

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return BadRequest(new { success = false, message = "Email dan password wajib diisi." });
                }

                var (success, message, user, matchedFoundation) = await _authRepo.LoginAsync(email, password, foundationSlug, foundationId);
                if (!success || user == null)
                {
                    return BadRequest(new { success = false, message });
                }

                var roles = await (from ur in _context.UserRoles
                                   where ur.UserId == user.Id
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   select (r.DisplayName ?? r.Name)).ToListAsync();

                var roleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (roles.Count == 0 || roleIds.Count == 0)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Akun Anda belum memiliki hak akses peran (User Roles). Silakan hubungi Administrator Software House untuk menetapkan peran (role) akun Anda terlebih dahulu sebelum dapat login." 
                    });
                }

                var directFoundations = await _context.Foundations
                    .Where(f => f.IsActive && f.UserId == user.Id)
                    .ToListAsync();

                var assignedFoundations = await (from fu in _context.FoundationUsers
                                                 where fu.UserId == user.Id && fu.IsActive
                                                 join f in _context.Foundations on fu.FoundationId equals f.Id
                                                 where f.IsActive
                                                 select f).ToListAsync();

                var userFoundations = directFoundations
                    .Concat(assignedFoundations)
                    .GroupBy(f => f.Id)
                    .Select(g => g.First())
                    .ToList();

                var activeFoundation = matchedFoundation ?? userFoundations.FirstOrDefault();

                var jwtToken = _tokenService.GenerateToken(
                    user, 
                    roles, 
                    roleIds, 
                    activeFoundation?.Id, 
                    activeFoundation?.Slug ?? activeFoundation?.Domain
                );

                var userDto = new UserAuthDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Roles = roles,
                    RoleIds = roleIds,
                    RoleId = roleIds.FirstOrDefault(),
                    RoleName = roles.FirstOrDefault() ?? "",
                    FoundationId = activeFoundation?.Id,
                    FoundationName = activeFoundation?.Name,
                    FoundationSlug = activeFoundation?.Slug,
                    FoundationDomain = activeFoundation?.Domain,
                    FoundationIds = userFoundations.Select(f => f.Id).ToList(),
                    Token = jwtToken
                };

                return Ok(new { 
                    success = true, 
                    message, 
                    token = jwtToken, 
                    tokenType = "Bearer",
                    expiresIn = 86400, // 24 jam (1 hari)
                    user = userDto 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.Login");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Token tidak valid atau klaim pengguna tidak ditemukan." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "Pengguna tidak ditemukan atau tidak aktif." });
                }

                var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        user.PhoneNumber,
                        roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.GetCurrentUser");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto? jsonModel)
        {
            try
            {
                string? name = jsonModel?.Name;
                string? email = jsonModel?.Email;
                string? password = jsonModel?.Password;
                string? phoneNumber = jsonModel?.PhoneNumber;
                string? foundationSlug = jsonModel?.FoundationSlug ?? jsonModel?.FoundationDomain;

                if (string.IsNullOrEmpty(name) && Request.HasFormContentType)
                {
                    name = Request.Form["Name"].FirstOrDefault() ?? Request.Form["name"].FirstOrDefault();
                    email = Request.Form["Email"].FirstOrDefault() ?? Request.Form["email"].FirstOrDefault();
                    password = Request.Form["Password"].FirstOrDefault() ?? Request.Form["password"].FirstOrDefault();
                    phoneNumber = Request.Form["PhoneNumber"].FirstOrDefault() ?? Request.Form["phoneNumber"].FirstOrDefault();
                    foundationSlug = Request.Form["FoundationSlug"].FirstOrDefault() ?? Request.Form["foundationSlug"].FirstOrDefault()
                        ?? Request.Form["FoundationDomain"].FirstOrDefault() ?? Request.Form["foundationDomain"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("foundation"))
                {
                    foundationSlug = Request.Query["foundation"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("domain"))
                {
                    foundationSlug = Request.Query["domain"].FirstOrDefault();
                }

                long? foundationId = jsonModel?.FoundationId;
                if (!foundationId.HasValue && Request.HasFormContentType && Request.Form.ContainsKey("FoundationId") && long.TryParse(Request.Form["FoundationId"].FirstOrDefault(), out var formFid))
                {
                    foundationId = formFid;
                }
                if (!foundationId.HasValue && Request.Query.ContainsKey("foundationId") && long.TryParse(Request.Query["foundationId"].FirstOrDefault(), out var qFid))
                {
                    foundationId = qFid;
                }

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return BadRequest(new { success = false, message = "Nama lengkap, email, dan password wajib diisi." });
                }

                if (password.Length < 6)
                {
                    return BadRequest(new { success = false, message = "Password minimal terdiri dari 6 karakter." });
                }

                var (success, message, user) = await _authRepo.RegisterDonorAsync(name, email, password, phoneNumber, foundationSlug, foundationId);
                if (!success || user == null)
                {
                    return BadRequest(new { success = false, message });
                }

                var roles = await (from ur in _context.UserRoles
                                   where ur.UserId == user.Id
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   select (r.DisplayName ?? r.Name)).ToListAsync();

                var roleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (roles.Count == 0) roles.Add("Donatur");
                if (roleIds.Count == 0) roleIds.Add(6);

                var userFoundations = await (from fu in _context.FoundationUsers
                                             where fu.UserId == user.Id && fu.IsActive
                                             join f in _context.Foundations on fu.FoundationId equals f.Id
                                             where f.IsActive
                                             select f).ToListAsync();

                var activeFoundation = userFoundations.FirstOrDefault();

                var userDto = new UserAuthDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Roles = roles,
                    RoleIds = roleIds,
                    RoleId = roleIds.FirstOrDefault(),
                    RoleName = roles.FirstOrDefault() ?? "Donatur",
                    FoundationId = activeFoundation?.Id,
                    FoundationName = activeFoundation?.Name,
                    FoundationSlug = activeFoundation?.Slug,
                    FoundationIds = userFoundations.Select(f => f.Id).ToList()
                };

                return Ok(new { success = true, message, user = userDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.Register");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        /// <summary>
        /// Mengembalikan daftar akun development untuk pengujian cepat (Quick Login).
        /// Akun diambil 100% dinamis dari database berdasarkan yayasan aktif.
        /// </summary>
        [HttpGet("dev-accounts")]
        public async Task<IActionResult> GetDevAccounts([FromQuery] long? foundationId)
        {
            try
            {
                var result = new List<object>();

                // 1. Akun Super Admin
                var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "super_admin" || r.DisplayName == "Super Admin");
                if (superAdminRole != null)
                {
                    var superAdmins = await (from ur in _context.UserRoles
                                             where ur.RoleId == superAdminRole.Id
                                             join u in _context.Users on ur.UserId equals u.Id
                                             where u.IsActive
                                             select new
                                             {
                                                 id = u.Id,
                                                 name = u.Name,
                                                 email = u.Email,
                                                 role = "Super Admin",
                                                 category = "superadmin",
                                                 badgeColor = "#4f46e5"
                                             }).Take(1).ToListAsync();
                    result.AddRange(superAdmins);
                }

                // 2. Akun Foundation Admin / Pengurus Yayasan ini
                if (foundationId.HasValue && foundationId.Value > 0)
                {
                    var targetF = await _context.Foundations.FirstOrDefaultAsync(f => f.Id == foundationId.Value && f.IsActive);
                    if (targetF != null)
                    {
                        var fAdmins = await (from fu in _context.FoundationUsers
                                             where fu.FoundationId == targetF.Id && fu.IsActive
                                             join u in _context.Users on fu.UserId equals u.Id
                                             where u.IsActive
                                             join ur in _context.UserRoles on u.Id equals ur.UserId into urs
                                             from ur in urs.DefaultIfEmpty()
                                             join r in _context.Roles on ur.RoleId equals r.Id into rs
                                             from r in rs.DefaultIfEmpty()
                                             where r != null && (r.Name.Contains("foundation") || r.DisplayName.Contains("Foundation") || r.DisplayName.Contains("Admin Yayasan") || r.Name.Contains("admin"))
                                             select new
                                             {
                                                 id = u.Id,
                                                 name = u.Name,
                                                 email = u.Email,
                                                 role = r.DisplayName ?? "Foundation Admin",
                                                 category = "foundation_admin",
                                                 badgeColor = "#0d9488"
                                             }).Take(2).ToListAsync();

                        if (fAdmins.Count == 0 && targetF.UserId.HasValue)
                        {
                            var picUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetF.UserId.Value && u.IsActive);
                            if (picUser != null)
                            {
                                result.Add(new
                                {
                                    id = picUser.Id,
                                    name = picUser.Name,
                                    email = picUser.Email,
                                    role = "Foundation Admin (PIC)",
                                    category = "foundation_admin",
                                    badgeColor = "#0d9488"
                                });
                            }
                        }
                        else
                        {
                            result.AddRange(fAdmins);
                        }

                        // 3. Akun Donatur yang terdaftar pada yayasan ini
                        var fDonors = await (from fu in _context.FoundationUsers
                                             where fu.FoundationId == targetF.Id && fu.IsActive
                                             join u in _context.Users on fu.UserId equals u.Id
                                             where u.IsActive
                                             join ur in _context.UserRoles on u.Id equals ur.UserId
                                             join r in _context.Roles on ur.RoleId equals r.Id
                                             where r.Name.Contains("donor") || r.DisplayName.Contains("Donatur")
                                             select new
                                             {
                                                 id = u.Id,
                                                 name = u.Name,
                                                 email = u.Email,
                                                 role = r.DisplayName ?? "Donatur",
                                                 category = "donor",
                                                 badgeColor = "#0284c7"
                                             }).Take(2).ToListAsync();
                        result.AddRange(fDonors);
                    }
                }

                // Jika belum ada donatur terdaftar di yayasan ini, ambil akun donatur aktif apa saja
                bool hasDonor = result.Any(x => (x.GetType().GetProperty("category")?.GetValue(x)?.ToString() ?? "") == "donor");
                if (!hasDonor)
                {
                    var donorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name.Contains("donor") || r.DisplayName.Contains("Donatur"));
                    if (donorRole != null)
                    {
                        var anyDonor = await (from ur in _context.UserRoles
                                              where ur.RoleId == donorRole.Id
                                              join u in _context.Users on ur.UserId equals u.Id
                                              where u.IsActive
                                              select new
                                              {
                                                  id = u.Id,
                                                  name = u.Name,
                                                  email = u.Email,
                                                  role = donorRole.DisplayName ?? "Donatur",
                                                  category = "donor",
                                                  badgeColor = "#0284c7"
                                              }).Take(1).ToListAsync();
                        result.AddRange(anyDonor);
                    }
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDevAccounts");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto? jsonModel)
        {
            try
            {
                string? email = jsonModel?.Email;
                string? foundationSlug = jsonModel?.FoundationSlug ?? jsonModel?.FoundationDomain;

                if (string.IsNullOrEmpty(email) && Request.HasFormContentType)
                {
                    email = Request.Form["Email"].FirstOrDefault() ?? Request.Form["email"].FirstOrDefault();
                    foundationSlug = Request.Form["FoundationSlug"].FirstOrDefault() ?? Request.Form["foundationSlug"].FirstOrDefault()
                        ?? Request.Form["FoundationDomain"].FirstOrDefault() ?? Request.Form["foundationDomain"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("foundation"))
                {
                    foundationSlug = Request.Query["foundation"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("tenant"))
                {
                    foundationSlug = Request.Query["tenant"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { success = false, message = "Email wajib diisi." });
                }

                var (success, message) = await _authRepo.SendForgotPasswordOtpAsync(email, foundationSlug);
                if (!success) return BadRequest(new { success = false, message });
                return Ok(new { success = true, message = "Kode OTP telah dikirim ke email Anda. Silakan periksa kotak masuk atau spam." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.ForgotPassword");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ForgotPasswordRequestDto? jsonModel)
        {
            try
            {
                string? email = jsonModel?.Email;
                string? foundationSlug = jsonModel?.FoundationSlug ?? jsonModel?.FoundationDomain;

                if (string.IsNullOrEmpty(email) && Request.HasFormContentType)
                {
                    email = Request.Form["Email"].FirstOrDefault() ?? Request.Form["email"].FirstOrDefault();
                    foundationSlug = Request.Form["FoundationSlug"].FirstOrDefault() ?? Request.Form["foundationSlug"].FirstOrDefault()
                        ?? Request.Form["FoundationDomain"].FirstOrDefault() ?? Request.Form["foundationDomain"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("foundation"))
                {
                    foundationSlug = Request.Query["foundation"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("tenant"))
                {
                    foundationSlug = Request.Query["tenant"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { success = false, message = "Email wajib diisi." });
                }

                var (success, message) = await _authRepo.SendForgotPasswordOtpAsync(email, foundationSlug);
                if (!success) return BadRequest(new { success = false, message });
                return Ok(new { success = true, message = "Kode OTP baru telah dikirimkan ke email Anda." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.ResendOtp");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto? jsonModel)
        {
            try
            {
                string? email = jsonModel?.Email;
                string? otp = jsonModel?.Otp;
                string? foundationSlug = jsonModel?.FoundationSlug ?? jsonModel?.FoundationDomain;

                if (string.IsNullOrEmpty(email) && Request.HasFormContentType)
                {
                    email = Request.Form["Email"].FirstOrDefault() ?? Request.Form["email"].FirstOrDefault();
                    otp = Request.Form["Otp"].FirstOrDefault() ?? Request.Form["otp"].FirstOrDefault();
                    foundationSlug = Request.Form["FoundationSlug"].FirstOrDefault() ?? Request.Form["foundationSlug"].FirstOrDefault()
                        ?? Request.Form["FoundationDomain"].FirstOrDefault() ?? Request.Form["foundationDomain"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("foundation"))
                {
                    foundationSlug = Request.Query["foundation"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("tenant"))
                {
                    foundationSlug = Request.Query["tenant"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                {
                    return BadRequest(new { valid = false, success = false, message = "Email dan kode OTP wajib diisi." });
                }

                var (success, message) = await _authRepo.VerifyOtpAsync(email, otp, foundationSlug);
                if (!success) return BadRequest(new { valid = false, success = false, message });
                return Ok(new { valid = true, success = true, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.VerifyOtp");
                return StatusCode(500, new { valid = false, success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { valid = false, success = false, message = "Token tidak boleh kosong." });
                }

                var result = await _authRepo.ValidateResetTokenAsync(token);
                if (!result) return BadRequest(new { valid = false, success = false, message = "Kode / link reset tidak valid atau sudah kedaluwarsa." });
                return Ok(new { valid = true, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.ValidateToken");
                return StatusCode(500, new { valid = false, success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto? jsonModel)
        {
            try
            {
                string? email = jsonModel?.Email;
                string? otp = jsonModel?.Otp;
                string? token = jsonModel?.Token;
                string? newPassword = jsonModel?.NewPassword;
                string? confirmPassword = jsonModel?.ConfirmPassword;
                string? foundationSlug = jsonModel?.FoundationSlug ?? jsonModel?.FoundationDomain;

                if (string.IsNullOrEmpty(newPassword) && Request.HasFormContentType)
                {
                    email = Request.Form["Email"].FirstOrDefault() ?? Request.Form["email"].FirstOrDefault();
                    otp = Request.Form["Otp"].FirstOrDefault() ?? Request.Form["otp"].FirstOrDefault();
                    token = Request.Form["Token"].FirstOrDefault() ?? Request.Form["token"].FirstOrDefault();
                    newPassword = Request.Form["NewPassword"].FirstOrDefault() ?? Request.Form["newPassword"].FirstOrDefault();
                    confirmPassword = Request.Form["ConfirmPassword"].FirstOrDefault() ?? Request.Form["confirmPassword"].FirstOrDefault();
                    foundationSlug = Request.Form["FoundationSlug"].FirstOrDefault() ?? Request.Form["foundationSlug"].FirstOrDefault()
                        ?? Request.Form["FoundationDomain"].FirstOrDefault() ?? Request.Form["foundationDomain"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("foundation"))
                {
                    foundationSlug = Request.Query["foundation"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(foundationSlug) && Request.Query.ContainsKey("tenant"))
                {
                    foundationSlug = Request.Query["tenant"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    return BadRequest(new { success = false, message = "Kata sandi baru wajib diisi." });
                }

                if (!string.IsNullOrEmpty(confirmPassword) && newPassword != confirmPassword)
                {
                    return BadRequest(new { success = false, message = "Kata sandi dan konfirmasi kata sandi tidak cocok." });
                }

                // Jalur 1: Menggunakan Email + Kode OTP
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(otp))
                {
                    var (successOtp, msgOtp) = await _authRepo.ResetPasswordWithOtpAsync(email, otp, newPassword, foundationSlug);
                    if (!successOtp) return BadRequest(new { success = false, message = msgOtp });
                    return Ok(new { success = true, message = msgOtp });
                }

                // Jalur 2: Menggunakan Token atau OTP di field token
                var effectiveToken = !string.IsNullOrEmpty(token) ? token : otp;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    var result = await _authRepo.ResetPasswordAsync(effectiveToken, newPassword);
                    if (!result) return BadRequest(new { success = false, message = "Gagal mereset password. Kode OTP atau token tidak valid atau sudah kedaluwarsa." });
                    return Ok(new { success = true, message = "Kata sandi Anda telah berhasil diubah." });
                }

                return BadRequest(new { success = false, message = "Kode OTP atau token wajib diisi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthController.ResetPassword");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
