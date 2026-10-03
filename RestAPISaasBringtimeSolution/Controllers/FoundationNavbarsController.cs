using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Models.DTOs;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoundationNavbarsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FoundationNavbarsController> _logger;

        public FoundationNavbarsController(AppDbContext context, ILogger<FoundationNavbarsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _context.FoundationNavbars
                    .OrderBy(fn => fn.FoundationId)
                    .ThenBy(fn => fn.OrderIndex)
                    .ToListAsync();

                var fIds = list.Select(fn => fn.FoundationId).Distinct().ToList();
                var foundations = await _context.Foundations.Where(f => fIds.Contains(f.Id)).ToDictionaryAsync(f => f.Id, f => f);

                var nIds = list.Where(fn => fn.NavbarId.HasValue).Select(fn => fn.NavbarId!.Value).Distinct().ToList();
                var navbars = await _context.Navbars.Where(n => nIds.Contains(n.Id)).ToDictionaryAsync(n => n.Id, n => n);

                var gIds = list.Where(fn => fn.NavbarGroupId.HasValue).Select(fn => fn.NavbarGroupId!.Value).Distinct().ToList();
                var groups = await _context.NavbarGroups.Where(g => gIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g);

                var childCounts = await _context.Navbars
                    .Where(n => n.NavbarGroupId.HasValue && gIds.Contains(n.NavbarGroupId.Value))
                    .GroupBy(n => n.NavbarGroupId!.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                var result = list.Select(fn =>
                {
                    foundations.TryGetValue(fn.FoundationId, out var found);
                    Navbar? nav = null;
                    if (fn.NavbarId.HasValue) navbars.TryGetValue(fn.NavbarId.Value, out nav);

                    NavbarGroup? grp = null;
                    if (fn.NavbarGroupId.HasValue) groups.TryGetValue(fn.NavbarGroupId.Value, out grp);

                    return new FoundationNavbarDto
                    {
                        Id = fn.Id,
                        FoundationId = fn.FoundationId,
                        FoundationName = found?.Name,
                        FoundationSlug = found?.Slug,
                        NavbarId = fn.NavbarId,
                        NavbarTitle = nav?.Title,
                        NavbarUrl = CleanNavbarUrl(nav?.Url),
                        NavbarIcon = nav?.IconClass,
                        NavbarGroupId = fn.NavbarGroupId,
                        GroupName = grp?.Name,
                        GroupIcon = grp?.IconClass,
                        ChildCount = (fn.NavbarGroupId.HasValue && childCounts.ContainsKey(fn.NavbarGroupId.Value)) ? childCounts[fn.NavbarGroupId.Value] : 0,
                        OrderIndex = fn.OrderIndex,
                        IsActive = fn.IsActive
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [HttpGet("foundation/{foundationId}")]
        public async Task<IActionResult> GetByFoundation([FromRoute] long foundationId)
        {
            try
            {
                var list = await _context.FoundationNavbars
                    .Where(fn => fn.FoundationId == foundationId)
                    .OrderBy(fn => fn.OrderIndex)
                    .ThenBy(fn => fn.Id)
                    .ToListAsync();

                var found = await _context.Foundations.FindAsync(foundationId);

                var nIds = list.Where(fn => fn.NavbarId.HasValue).Select(fn => fn.NavbarId!.Value).Distinct().ToList();
                var navbars = await _context.Navbars.Where(n => nIds.Contains(n.Id)).ToDictionaryAsync(n => n.Id, n => n);

                var gIds = list.Where(fn => fn.NavbarGroupId.HasValue).Select(fn => fn.NavbarGroupId!.Value).Distinct().ToList();
                var groups = await _context.NavbarGroups.Where(g => gIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g);

                var childCounts = await _context.Navbars
                    .Where(n => n.NavbarGroupId.HasValue && gIds.Contains(n.NavbarGroupId.Value))
                    .GroupBy(n => n.NavbarGroupId!.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                var result = list.Select(fn =>
                {
                    Navbar? nav = null;
                    if (fn.NavbarId.HasValue) navbars.TryGetValue(fn.NavbarId.Value, out nav);

                    NavbarGroup? grp = null;
                    if (fn.NavbarGroupId.HasValue) groups.TryGetValue(fn.NavbarGroupId.Value, out grp);

                    return new FoundationNavbarDto
                    {
                        Id = fn.Id,
                        FoundationId = fn.FoundationId,
                        FoundationName = found?.Name,
                        FoundationSlug = found?.Slug,
                        NavbarId = fn.NavbarId,
                        NavbarTitle = nav?.Title,
                        NavbarUrl = CleanNavbarUrl(nav?.Url),
                        NavbarIcon = nav?.IconClass,
                        NavbarGroupId = fn.NavbarGroupId,
                        GroupName = grp?.Name,
                        GroupIcon = grp?.IconClass,
                        ChildCount = (fn.NavbarGroupId.HasValue && childCounts.ContainsKey(fn.NavbarGroupId.Value)) ? childCounts[fn.NavbarGroupId.Value] : 0,
                        OrderIndex = fn.OrderIndex,
                        IsActive = fn.IsActive
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.GetByFoundation");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        /// <summary>
        /// Public API: Menghasilkan struktur JSON Navbar lengkap yang siap dirender di web publik (Port 9007).
        /// Mendukung menu mandiri dan dropdown group dengan anak-anaknya.
        /// </summary>
        [HttpGet("public/{tenantOrSlug}")]
        public async Task<IActionResult> GetPublicNavbar([FromRoute] string tenantOrSlug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantOrSlug))
                {
                    return BadRequest(new { success = false, message = "Tenant atau slug wajib diisi" });
                }

                var clean = tenantOrSlug.Trim().ToLower();

                int? numericId = null;
                if (int.TryParse(clean, out var parsedId))
                {
                    numericId = parsedId;
                }

                var foundation = await _context.Foundations.FirstOrDefaultAsync(f =>
                    (numericId.HasValue && f.Id == numericId.Value) ||
                    (f.Slug != null && f.Slug.ToLower() == clean) ||
                    (f.Domain != null && f.Domain.ToLower() == clean));

                if (foundation == null)
                {
                    // Fallback ke default 5 menu standar
                    return Ok(GetDefaultPublicNavbars(clean));
                }

                var hasAnyConfig = await _context.FoundationNavbars.AnyAsync(fn => fn.FoundationId == foundation.Id);
                if (!hasAnyConfig)
                {
                    return Ok(GetDefaultPublicNavbars(foundation.Slug ?? clean));
                }

                var assignments = await _context.FoundationNavbars
                    .Where(fn => fn.FoundationId == foundation.Id && fn.IsActive)
                    .OrderBy(fn => fn.OrderIndex)
                    .ThenBy(fn => fn.Id)
                    .ToListAsync();

                if (assignments.Count == 0)
                {
                    return Ok(new List<PublicNavbarItemDto>());
                }

                var nIds = assignments.Where(fn => fn.NavbarId.HasValue).Select(fn => fn.NavbarId!.Value).Distinct().ToList();
                var navbars = await _context.Navbars.Where(n => nIds.Contains(n.Id) && n.IsActive).ToDictionaryAsync(n => n.Id, n => n);

                var gIds = assignments.Where(fn => fn.NavbarGroupId.HasValue).Select(fn => fn.NavbarGroupId!.Value).Distinct().ToList();
                var groups = await _context.NavbarGroups.Where(g => gIds.Contains(g.Id) && g.IsActive).ToDictionaryAsync(g => g.Id, g => g);

                var allChildNavbars = await _context.Navbars
                    .Where(n => n.NavbarGroupId.HasValue && gIds.Contains(n.NavbarGroupId.Value) && n.IsActive)
                    .OrderBy(n => n.OrderIndex)
                    .ToListAsync();

                var childNavbarsGrouped = allChildNavbars
                    .GroupBy(n => n.NavbarGroupId!.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var publicItems = new List<PublicNavbarItemDto>();
                var targetTenant = foundation.Slug ?? clean;

                foreach (var assign in assignments)
                {
                    if (assign.NavbarId.HasValue && navbars.TryGetValue(assign.NavbarId.Value, out var nav))
                    {
                        // Menu Mandiri (clean MVC URL)
                        var resolvedUrl = CleanNavbarUrl(nav.Url);

                        publicItems.Add(new PublicNavbarItemDto
                        {
                            Id = nav.Id,
                            Title = nav.Title,
                            Url = resolvedUrl,
                            IconClass = nav.IconClass,
                            Target = nav.Target ?? "_self",
                            IsDropdown = false,
                            OrderIndex = assign.OrderIndex,
                            DropdownItems = new List<PublicNavbarItemDto>()
                        });
                    }
                    else if (assign.NavbarGroupId.HasValue && groups.TryGetValue(assign.NavbarGroupId.Value, out var grp))
                    {
                        // Dropdown Group
                        var childList = new List<PublicNavbarItemDto>();
                        if (childNavbarsGrouped.TryGetValue(grp.Id, out var children))
                        {
                            foreach (var ch in children)
                            {
                                var childUrl = CleanNavbarUrl(ch.Url);

                                childList.Add(new PublicNavbarItemDto
                                {
                                    Id = ch.Id,
                                    Title = ch.Title,
                                    Url = childUrl,
                                    IconClass = ch.IconClass,
                                    Target = ch.Target ?? "_self",
                                    IsDropdown = false,
                                    OrderIndex = ch.OrderIndex
                                });
                            }
                        }

                        publicItems.Add(new PublicNavbarItemDto
                        {
                            Id = grp.Id,
                            Title = grp.Name,
                            Url = "#",
                            IconClass = grp.IconClass,
                            Target = "_self",
                            IsDropdown = true,
                            OrderIndex = assign.OrderIndex,
                            DropdownItems = childList
                        });
                    }
                }

                if (publicItems.Count == 0 && !hasAnyConfig)
                {
                    return Ok(GetDefaultPublicNavbars(targetTenant));
                }

                return Ok(publicItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.GetPublicNavbar for {Tenant}", tenantOrSlug);
                return Ok(GetDefaultPublicNavbars(tenantOrSlug));
            }
        }

        private static List<PublicNavbarItemDto> GetDefaultPublicNavbars(string tenant)
        {
            return new List<PublicNavbarItemDto>
            {
                new() { Id = 1, Title = "Beranda", Url = "/", IconClass = "fas fa-home", OrderIndex = 1, IsDropdown = false },
                new() { Id = 2, Title = "Profile", Url = "/profile", IconClass = "fas fa-building", OrderIndex = 2, IsDropdown = false },
                new() { Id = 3, Title = "Program Donasi", Url = "/campaigns", IconClass = "fas fa-hand-holding-heart", OrderIndex = 3, IsDropdown = false },
                new() { Id = 4, Title = "Untaian Doa", Url = "/prayers", IconClass = "fas fa-hands-praying", OrderIndex = 4, IsDropdown = false },
                new() { Id = 5, Title = "Tanya Jawab (FAQ)", Url = "/faq", IconClass = "fas fa-circle-question", OrderIndex = 5, IsDropdown = false }
            };
        }

        public static string CleanNavbarUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "/";
            var cleaned = url.Trim()
                .Replace("/{tenant}", "")
                .Replace("{tenant}", "")
                .Replace("/{slug}", "")
                .Replace("{slug}", "");

            if (string.IsNullOrWhiteSpace(cleaned)) return "/";

            if (!cleaned.StartsWith("http://") && !cleaned.StartsWith("https://") && !cleaned.StartsWith("/") && !cleaned.StartsWith("#"))
            {
                cleaned = "/" + cleaned;
            }
            return cleaned;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFoundationNavbarDto model)
        {
            try
            {
                if (model == null || model.FoundationId <= 0)
                {
                    return BadRequest(new { success = false, message = "Foundation ID wajib diisi" });
                }

                if (!model.NavbarId.HasValue && !model.NavbarGroupId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Pilih salah satu: Butir Menu Mandiri ATAU Grup Dropdown" });
                }

                var entity = new FoundationNavbar
                {
                    FoundationId = model.FoundationId,
                    NavbarId = model.NavbarId > 0 ? model.NavbarId : null,
                    NavbarGroupId = model.NavbarGroupId > 0 ? model.NavbarGroupId : null,
                    OrderIndex = model.OrderIndex,
                    IsActive = model.IsActive,
                    Created_at = DateTime.UtcNow
                };

                await _context.FoundationNavbars.AddAsync(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Navbar berhasil ditambahkan ke yayasan", data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.Create");
                return StatusCode(500, new { success = false, message = "Gagal menambahkan navbar: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] EditFoundationNavbarDto model)
        {
            try
            {
                var entity = await _context.FoundationNavbars.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Data navbar yayasan tidak ditemukan" });

                entity.OrderIndex = model.OrderIndex;
                entity.IsActive = model.IsActive;
                entity.Updated_at = DateTime.UtcNow;

                _context.FoundationNavbars.Update(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Data navbar yayasan berhasil diperbarui", data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.Update");
                return StatusCode(500, new { success = false, message = "Gagal memperbarui: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            try
            {
                var entity = await _context.FoundationNavbars.FindAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Data navbar yayasan tidak ditemukan" });

                _context.FoundationNavbars.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Navbar yayasan berhasil dihapus" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.Delete");
                return StatusCode(500, new { success = false, message = "Gagal menghapus: " + ex.Message });
            }
        }

        [HttpPost("bulk-sync")]
        public async Task<IActionResult> BulkSync([FromBody] BulkSyncFoundationNavbarDto model)
        {
            try
            {
                if (model == null || model.FoundationId <= 0)
                {
                    return BadRequest(new { success = false, message = "Data yayasan wajib diisi" });
                }

                // Hapus data lama yayasan ini
                var oldItems = await _context.FoundationNavbars.Where(fn => fn.FoundationId == model.FoundationId).ToListAsync();
                _context.FoundationNavbars.RemoveRange(oldItems);

                var newEntities = new List<FoundationNavbar>();
                var order = 1;
                foreach (var item in model.Items)
                {
                    if (!item.NavbarId.HasValue && !item.NavbarGroupId.HasValue) continue;

                    newEntities.Add(new FoundationNavbar
                    {
                        FoundationId = model.FoundationId,
                        NavbarId = item.NavbarId > 0 ? item.NavbarId : null,
                        NavbarGroupId = item.NavbarGroupId > 0 ? item.NavbarGroupId : null,
                        OrderIndex = item.OrderIndex > 0 ? item.OrderIndex : order++,
                        IsActive = item.IsActive,
                        Created_at = DateTime.UtcNow
                    });
                }

                if (newEntities.Count > 0)
                {
                    await _context.FoundationNavbars.AddRangeAsync(newEntities);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Konfigurasi navbar yayasan berhasil disinkronisasi", count = newEntities.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.BulkSync");
                return StatusCode(500, new { success = false, message = "Gagal sinkronisasi: " + ex.Message });
            }
        }

        [HttpPost("reset-default/{foundationId}")]
        public async Task<IActionResult> ResetDefault([FromRoute] long foundationId)
        {
            try
            {
                var found = await _context.Foundations.FindAsync(foundationId);
                if (found == null) return NotFound(new { success = false, message = "Yayasan tidak ditemukan" });

                var old = await _context.FoundationNavbars.Where(fn => fn.FoundationId == foundationId).ToListAsync();
                _context.FoundationNavbars.RemoveRange(old);

                // Cari 5 menu default di tabel navbars
                var defaultTitles = new[] { "Beranda", "Profile", "Program Donasi", "Untaian Doa", "Tanya Jawab (FAQ)" };
                var defaultNavs = await _context.Navbars
                    .Where(n => defaultTitles.Contains(n.Title) && n.NavbarGroupId == null)
                    .ToListAsync();

                var order = 1;
                foreach (var title in defaultTitles)
                {
                    var nav = defaultNavs.FirstOrDefault(n => n.Title == title);
                    if (nav != null)
                    {
                        await _context.FoundationNavbars.AddAsync(new FoundationNavbar
                        {
                            FoundationId = foundationId,
                            NavbarId = nav.Id,
                            NavbarGroupId = null,
                            OrderIndex = order++,
                            IsActive = true,
                            Created_at = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = $"Navbar {found.Name} berhasil di-reset ke default" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationNavbarsController.ResetDefault");
                return StatusCode(500, new { success = false, message = "Gagal reset: " + ex.Message });
            }
        }
    }
}
