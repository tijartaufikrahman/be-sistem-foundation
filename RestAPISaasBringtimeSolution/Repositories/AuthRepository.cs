using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(
            AppDbContext context,
            IConfiguration config,
            IEmailService emailService,
            ILogger<AuthRepository> logger)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, User? User, Foundation? TargetFoundation)> LoginAsync(string email, string password, string? foundationSlug = null, long? foundationId = null)
        {
            try
            {
                var cleanEmail = email.Trim().ToLower();
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail && u.IsActive);

                if (user == null)
                {
                    return (false, "Email atau kata sandi salah. Silakan coba lagi.", null, null);
                }

                if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return (false, "Email atau kata sandi salah. Silakan coba lagi.", null, null);
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
                    return (false, "Akun Anda belum memiliki hak akses peran (User Roles). Silakan hubungi Administrator Software House untuk menetapkan peran akun Anda terlebih dahulu.", null, null);
                }

                bool isSuperAdmin = roles.Any(r => r.Equals("Super Admin", StringComparison.OrdinalIgnoreCase) || 
                                                   r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                                                   r.Equals("super_admin", StringComparison.OrdinalIgnoreCase));

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

                var allFoundations = await _context.Foundations
                    .Where(f => f.IsActive)
                    .ToListAsync();

                Foundation? targetFoundation = null;
                if (foundationId.HasValue && foundationId.Value > 0)
                {
                    targetFoundation = allFoundations.FirstOrDefault(f => f.Id == foundationId.Value);
                }
                if (targetFoundation == null && !string.IsNullOrWhiteSpace(foundationSlug))
                {
                    targetFoundation = allFoundations.FirstOrDefault(f => MatchesTenant(f, foundationSlug));
                }

                // 1. JIKA LOGIN DILAKUKAN MELALUI WEBSITE YAYASAN TERTENTU (misal: foundationId dari appsettings.json atau slug portal)
                if (targetFoundation != null)
                {
                    // Cek apakah user terdaftar pada yayasan ini (hak akses diatur per yayasan, Super Admin diizinkan lintas yayasan)
                    bool isMemberOfFoundation = isSuperAdmin
                        || (targetFoundation.UserId.HasValue && targetFoundation.UserId.Value == user.Id)
                        || userFoundations.Any(f => f.Id == targetFoundation.Id);

                    if (!isMemberOfFoundation)
                    {
                        var otherFoundation = userFoundations.FirstOrDefault();
                        if (otherFoundation != null)
                        {
                            var otherDomainInfo = !string.IsNullOrEmpty(otherFoundation.Domain) ? $" ({otherFoundation.Domain})" : "";
                            return (false, $"Akses ditolak: Akun Anda terdaftar pada '{otherFoundation.Name}'{otherDomainInfo} dan tidak memiliki izin akses ke portal '{targetFoundation.Name}'. Silakan masuk melalui domain/portal resmi yayasan Anda.", null, targetFoundation);
                        }
                        else
                        {
                            return (false, $"Akses ditolak: Akun Anda ({user.Email}) tidak terdaftar pada portal '{targetFoundation.Name}'. Silakan buat akun donatur di portal yayasan ini.", null, targetFoundation);
                        }
                    }

                    return (true, $"Login berhasil ke portal resmi {targetFoundation.Name}.", user, targetFoundation);
                }
                else if (!string.IsNullOrWhiteSpace(foundationSlug))
                {
                    return (false, $"Portal yayasan '{foundationSlug}' tidak ditemukan atau sedang tidak aktif.", null, null);
                }
                else
                {
                    // 2. JIKA LOGIN DILAKUKAN MELALUI PORTAL UTAMA SOFTWARE HOUSE (tanpa yayasan /Account/Login):
                    // Hanya Super Admin yang boleh login langsung di portal Software House.
                    // Pengurus yayasan atau donatur harus diarahkan ke portal yayasan masing-masing!
                    if (!isSuperAdmin)
                    {
                        var userFound = userFoundations.FirstOrDefault();
                        if (userFound != null)
                        {
                            var targetSlug = !string.IsNullOrEmpty(userFound.Slug) ? userFound.Slug : userFound.Domain;
                            return (false, $"Akses ditolak: Akun Anda adalah akun terdaftar di '{userFound.Name}'. Silakan login melalui portal resmi yayasan Anda di: /{targetSlug}/login", null, null);
                        }
                        else
                        {
                            return (false, "Akses ditolak: Akun Anda belum terdaftar pada yayasan manapun. Silakan daftar melalui portal yayasan mitra.", null, null);
                        }
                    }

                    return (true, "Login berhasil ke portal Software House Bringtime Solution.", user, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthRepository.LoginAsync for email: {Email}", email);
                throw;
            }
        }

        private static bool MatchesTenant(Foundation f, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var clean = input.Trim().ToLowerInvariant();
            if (long.TryParse(clean, out var id) && f.Id == id) return true;
            if (!string.IsNullOrEmpty(f.Slug) && f.Slug.Trim().ToLowerInvariant() == clean) return true;
            if (!string.IsNullOrEmpty(f.Domain) && f.Domain.Trim().ToLowerInvariant() == clean) return true;
            return false;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterDonorAsync(string name, string email, string password, string? phoneNumber, string? foundationSlug = null, long? foundationId = null)
        {
            try
            {
                var cleanEmail = email.Trim().ToLower();
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);
                if (existing != null)
                {
                    return (false, "Email sudah terdaftar. Silakan gunakan email lain atau masuk.", null);
                }

                Foundation? targetFoundation = null;
                var allFoundations = await _context.Foundations.Where(f => f.IsActive).ToListAsync();
                if (foundationId.HasValue && foundationId.Value > 0)
                {
                    targetFoundation = allFoundations.FirstOrDefault(f => f.Id == foundationId.Value);
                }
                if (targetFoundation == null && !string.IsNullOrWhiteSpace(foundationSlug))
                {
                    targetFoundation = allFoundations.FirstOrDefault(f => MatchesTenant(f, foundationSlug));
                    if (targetFoundation == null)
                    {
                        return (false, $"Yayasan mitra dengan kode/domain '{foundationSlug}' tidak ditemukan.", null);
                    }
                }

                var newUser = new User
                {
                    Name = name.Trim(),
                    Email = cleanEmail,
                    PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    AuthProvider = "local",
                    IsActive = true
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                var donorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "donor" || r.DisplayName == "Donatur")
                                ?? await _context.Roles.FirstOrDefaultAsync(r => r.Id == 6 || r.Id == 3);

                if (donorRole != null)
                {
                    await _context.UserRoles.AddAsync(new UserRole
                    {
                        UserId = newUser.Id,
                        RoleId = donorRole.Id
                    });
                    await _context.SaveChangesAsync();
                }

                // Jika mendaftar melalui situs publik yayasan tertentu, hubungkan donatur ke yayasan tersebut
                if (targetFoundation != null)
                {
                    await _context.FoundationUsers.AddAsync(new FoundationUser
                    {
                        FoundationId = targetFoundation.Id,
                        UserId = newUser.Id,
                        IsActive = true,
                        Created_at = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }

                var loadedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == newUser.Id);

                string successMessage = targetFoundation != null
                    ? $"Pendaftaran donatur resmi di {targetFoundation.Name} berhasil! Selamat bergabung."
                    : "Pendaftaran donatur berhasil! Selamat bergabung.";

                return (true, successMessage, loadedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthRepository.RegisterDonorAsync for email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl)
        {
            var result = await SendForgotPasswordOtpAsync(email);
            return result.Success;
        }

        public async Task<(bool Success, string Message)> SendForgotPasswordOtpAsync(string email, string? foundationSlug = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return (false, "Email wajib diisi.");
                }

                Foundation? targetFoundation = null;
                if (!string.IsNullOrWhiteSpace(foundationSlug))
                {
                    var allFoundations = await _context.Foundations.Where(f => f.IsActive).ToListAsync();
                    targetFoundation = allFoundations.FirstOrDefault(f => MatchesTenant(f, foundationSlug));
                    if (targetFoundation == null)
                    {
                        return (false, $"Portal yayasan '{foundationSlug}' tidak ditemukan atau sedang tidak aktif.");
                    }
                }

                var cleanEmail = email.Trim().ToLower();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail && u.IsActive);
                if (user == null)
                {
                    _logger.LogWarning("SendForgotPasswordOtpAsync: Pengguna dengan email {Email} tidak ditemukan atau tidak aktif.", email);
                    return (true, "Jika email terdaftar pada sistem, kode OTP telah dikirimkan.");
                }

                if (targetFoundation != null)
                {
                    bool isMemberOfFoundation = (targetFoundation.UserId.HasValue && targetFoundation.UserId.Value == user.Id)
                        || await _context.FoundationUsers.AnyAsync(fu => fu.UserId == user.Id && fu.FoundationId == targetFoundation.Id && fu.IsActive);

                    if (!isMemberOfFoundation)
                    {
                        return (false, $"Akses ditolak: Akun ({user.Email}) tidak terdaftar pada portal '{targetFoundation.Name}'.");
                    }
                }

                // Buat 6-digit kode OTP angka acak
                var otp = Random.Shared.Next(100000, 999999).ToString();
                user.ResetToken = otp;
                user.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15); // Masa berlaku OTP 15 menit

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                string? foundationName = targetFoundation?.Name;
                if (string.IsNullOrEmpty(foundationName))
                {
                    foundationName = await (from fu in _context.FoundationUsers
                                            where fu.UserId == user.Id && fu.IsActive
                                            join f in _context.Foundations on fu.FoundationId equals f.Id
                                            where f.IsActive
                                            select f.Name).FirstOrDefaultAsync();
                }

                var sent = await _emailService.SendResetPasswordOtpEmailAsync(user.Email, user.Name, otp, foundationName);
                if (!sent)
                {
                    _logger.LogError("Gagal mengirim email OTP reset password via IEmailService ke {Email}", user.Email);
                    user.ResetToken = null;
                    user.ResetTokenExpiresAt = null;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    return (false, "Gagal mengirimkan email kode OTP. Silakan periksa koneksi server email.");
                }

                return (true, "Kode OTP telah berhasil dikirim ke email Anda.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthRepository.SendForgotPasswordOtpAsync for email: {Email}", email);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> VerifyOtpAsync(string email, string otp, string? foundationSlug = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                {
                    return (false, "Email dan kode OTP wajib diisi.");
                }

                var cleanEmail = email.Trim().ToLower();
                var cleanOtp = otp.Trim();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail && u.IsActive);
                if (user == null)
                {
                    return (false, "Pengguna tidak ditemukan atau tidak aktif.");
                }

                if (!string.IsNullOrWhiteSpace(foundationSlug))
                {
                    var allFoundations = await _context.Foundations.Where(f => f.IsActive).ToListAsync();
                    var targetFoundation = allFoundations.FirstOrDefault(f => MatchesTenant(f, foundationSlug));
                    if (targetFoundation != null)
                    {
                        bool isMemberOfFoundation = (targetFoundation.UserId.HasValue && targetFoundation.UserId.Value == user.Id)
                            || await _context.FoundationUsers.AnyAsync(fu => fu.UserId == user.Id && fu.FoundationId == targetFoundation.Id && fu.IsActive);

                        if (!isMemberOfFoundation)
                        {
                            return (false, $"Akses ditolak: Akun Anda tidak terdaftar pada portal '{targetFoundation.Name}'.");
                        }
                    }
                }

                if (string.IsNullOrEmpty(user.ResetToken) || !string.Equals(user.ResetToken.Trim(), cleanOtp, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Kode OTP salah atau tidak cocok.");
                }

                if (user.ResetTokenExpiresAt == null || user.ResetTokenExpiresAt < DateTime.UtcNow)
                {
                    return (false, "Kode OTP telah kedaluwarsa. Silakan minta kode OTP baru.");
                }

                return (true, "Kode OTP valid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthRepository.VerifyOtpAsync");
                throw;
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordWithOtpAsync(string email, string otp, string newPassword, string? foundationSlug = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword))
                {
                    return (false, "Email, kode OTP, dan kata sandi baru wajib diisi.");
                }

                if (newPassword.Length < 6)
                {
                    return (false, "Kata sandi baru minimal 6 karakter.");
                }

                var cleanEmail = email.Trim().ToLower();
                var cleanOtp = otp.Trim();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail && u.IsActive);
                if (user == null)
                {
                    return (false, "Pengguna tidak ditemukan.");
                }

                if (!string.IsNullOrWhiteSpace(foundationSlug))
                {
                    var allFoundations = await _context.Foundations.Where(f => f.IsActive).ToListAsync();
                    var targetFoundation = allFoundations.FirstOrDefault(f => MatchesTenant(f, foundationSlug));
                    if (targetFoundation != null)
                    {
                        bool isMemberOfFoundation = (targetFoundation.UserId.HasValue && targetFoundation.UserId.Value == user.Id)
                            || await _context.FoundationUsers.AnyAsync(fu => fu.UserId == user.Id && fu.FoundationId == targetFoundation.Id && fu.IsActive);

                        if (!isMemberOfFoundation)
                        {
                            return (false, $"Akses ditolak: Akun Anda tidak terdaftar pada portal '{targetFoundation.Name}'.");
                        }
                    }
                }

                if (string.IsNullOrEmpty(user.ResetToken) || !string.Equals(user.ResetToken.Trim(), cleanOtp, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Kode OTP yang Anda masukkan salah.");
                }

                if (user.ResetTokenExpiresAt == null || user.ResetTokenExpiresAt < DateTime.UtcNow)
                {
                    return (false, "Kode OTP telah kedaluwarsa. Silakan minta kode OTP baru.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.ResetToken = null;
                user.ResetTokenExpiresAt = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return (true, "Kata sandi Anda telah berhasil diperbarui.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthRepository.ResetPasswordWithOtpAsync");
                throw;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            try
            {
                var clean = token?.Trim();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == clean);
                if (user == null || user.ResetTokenExpiresAt < DateTime.UtcNow)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthRepository.ValidateResetTokenAsync");
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                var clean = token?.Trim();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == clean);
                if (user == null || user.ResetTokenExpiresAt < DateTime.UtcNow)
                    return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.ResetToken = null;
                user.ResetTokenExpiresAt = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AuthRepository.ResetPasswordAsync");
                throw;
            }
        }
    }
}
