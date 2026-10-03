using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoundationsController : ControllerBase
    {
        private readonly IFoundationRepository _foundationRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<FoundationsController> _logger;

        public FoundationsController(IFoundationRepository foundationRepo, AppDbContext context, ILogger<FoundationsController> logger)
        {
            _foundationRepo = foundationRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _foundationRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var foundations = await _context.Foundations.OrderByDescending(f => f.Id).ToListAsync();
                var foundationIds = foundations.Select(f => f.Id).ToList();

                var fuList = await (from fu in _context.FoundationUsers
                                    where foundationIds.Contains(fu.FoundationId) && fu.IsActive
                                    join u in _context.Users on fu.UserId equals u.Id into users
                                    from u in users.DefaultIfEmpty()
                                    select new
                                    {
                                        fu.Id,
                                        fu.FoundationId,
                                        fu.UserId,
                                        fu.IsActive,
                                        fu.Created_at,
                                        User = u != null ? new { u.Id, u.Name, u.Email } : null,
                                        UserName = u != null ? u.Name : null
                                    }).ToListAsync();

                var fuGrouped = fuList.GroupBy(x => x.FoundationId).ToDictionary(g => g.Key, g => g.Cast<object>().ToList());
                var typesLookup = await _context.FoundationTypes.ToDictionaryAsync(t => t.Id, t => t.Name);

                var result = foundations.Select(f => new
                {
                    f.Id,
                    f.FoundationTypeId,
                    FoundationTypeName = (f.FoundationTypeId.HasValue && typesLookup.ContainsKey(f.FoundationTypeId.Value)) ? typesLookup[f.FoundationTypeId.Value] : null,
                    f.UserId,
                    f.Name,
                    f.Slug,
                    f.Domain,
                    f.LogoUrl,
                    f.BannerUrl,
                    f.Bio,
                    f.Address,
                    f.WhatsappNumber,
                    f.LegalPermitNumber,
                    f.BankName,
                    f.BankAccountNumber,
                    f.BankAccountHolder,
                    f.IsActive,
                    f.Created_at,
                    f.Updated_at,
                    FoundationUsers = fuGrouped.ContainsKey(f.Id) ? fuGrouped[f.Id] : new List<object>()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var f = await _context.Foundations.FirstOrDefaultAsync(f => f.Id == id);
                if (f == null) return NotFound(new { success = false, message = "Yayasan tidak ditemukan" });

                var fuList = await (from fu in _context.FoundationUsers
                                    where fu.FoundationId == id && fu.IsActive
                                    join u in _context.Users on fu.UserId equals u.Id into users
                                    from u in users.DefaultIfEmpty()
                                    select new
                                    {
                                        fu.Id,
                                        fu.FoundationId,
                                        fu.UserId,
                                        fu.IsActive,
                                        fu.Created_at,
                                        User = u != null ? new { u.Id, u.Name, u.Email } : null,
                                        UserName = u != null ? u.Name : null
                                    }).ToListAsync();

                var result = new
                {
                    f.Id,
                    f.UserId,
                    f.Name,
                    f.Slug,
                    f.Domain,
                    f.LogoUrl,
                    f.BannerUrl,
                    f.Bio,
                    f.Address,
                    f.WhatsappNumber,
                    f.LegalPermitNumber,
                    f.BankName,
                    f.BankAccountNumber,
                    f.BankAccountHolder,
                    f.IsActive,
                    f.Created_at,
                    f.Updated_at,
                    FoundationUsers = fuList
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug)) return BadRequest(new { success = false, message = "Slug wajib diisi" });
                var clean = slug.Trim().ToLower();
                var f = await _context.Foundations.FirstOrDefaultAsync(f => (f.Slug != null && f.Slug.ToLower() == clean) || (f.Domain != null && f.Domain.ToLower() == clean));
                if (f == null)
                {
                    return NotFound(new { success = false, message = "Organisasi/Yayasan tidak ditemukan" });
                }

                var fuList = await (from fu in _context.FoundationUsers
                                    where fu.FoundationId == f.Id && fu.IsActive
                                    join u in _context.Users on fu.UserId equals u.Id into users
                                    from u in users.DefaultIfEmpty()
                                    select new
                                    {
                                        fu.Id,
                                        fu.FoundationId,
                                        fu.UserId,
                                        fu.IsActive,
                                        fu.Created_at,
                                        User = u != null ? new { u.Id, u.Name, u.Email } : null,
                                        UserName = u != null ? u.Name : null
                                    }).ToListAsync();

                return Ok(new
                {
                    f.Id,
                    f.UserId,
                    f.Name,
                    f.Slug,
                    f.Domain,
                    f.LogoUrl,
                    f.BannerUrl,
                    f.Bio,
                    f.Address,
                    f.WhatsappNumber,
                    f.LegalPermitNumber,
                    f.BankName,
                    f.BankAccountNumber,
                    f.BankAccountHolder,
                    f.IsActive,
                    f.Created_at,
                    f.Updated_at,
                    FoundationUsers = fuList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.GetBySlug for Slug {Slug}", slug);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("domain/{domain}")]
        public async Task<IActionResult> GetByDomain([FromRoute] string domain)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(domain)) return BadRequest(new { success = false, message = "Domain wajib diisi" });
                var clean = domain.Trim().ToLower();
                var f = await _context.Foundations.FirstOrDefaultAsync(f => (f.Domain != null && f.Domain.ToLower() == clean) || (f.Slug != null && f.Slug.ToLower() == clean));
                if (f == null)
                {
                    return NotFound(new { success = false, message = "Organisasi/Yayasan tidak ditemukan" });
                }

                var fuList = await (from fu in _context.FoundationUsers
                                    where fu.FoundationId == f.Id && fu.IsActive
                                    join u in _context.Users on fu.UserId equals u.Id into users
                                    from u in users.DefaultIfEmpty()
                                    select new
                                    {
                                        fu.Id,
                                        fu.FoundationId,
                                        fu.UserId,
                                        fu.IsActive,
                                        fu.Created_at,
                                        User = u != null ? new { u.Id, u.Name, u.Email } : null,
                                        UserName = u != null ? u.Name : null
                                    }).ToListAsync();

                return Ok(new
                {
                    f.Id,
                    f.UserId,
                    f.Name,
                    f.Slug,
                    f.Domain,
                    f.LogoUrl,
                    f.BannerUrl,
                    f.Bio,
                    f.Address,
                    f.WhatsappNumber,
                    f.LegalPermitNumber,
                    f.BankName,
                    f.BankAccountNumber,
                    f.BankAccountHolder,
                    f.IsActive,
                    f.Created_at,
                    f.Updated_at,
                    FoundationUsers = fuList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.GetByDomain for Domain {Domain}", domain);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFoundationDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { success = false, message = "Data organisasi tidak boleh kosong." });

                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest(new { success = false, message = "Nama organisasi wajib diisi." });

                if (string.IsNullOrWhiteSpace(model.Slug))
                    return BadRequest(new { success = false, message = "Slug URL wajib diisi." });

                var cleanSlug = model.Slug.Trim().ToLower();

                // Cek duplikasi slug
                var existingBySlug = await _foundationRepo.GetBySlugAsync(cleanSlug);
                if (existingBySlug != null)
                {
                    return BadRequest(new { success = false, message = $"Slug URL '{cleanSlug}' sudah digunakan oleh organisasi lain. Silakan pilih slug yang berbeda." });
                }

                // Cek duplikasi domain jika diisi
                if (!string.IsNullOrWhiteSpace(model.Domain))
                {
                    var cleanDomain = model.Domain.Trim().ToLower();
                    var existingByDomain = await _foundationRepo.GetByDomainAsync(cleanDomain);
                    if (existingByDomain != null)
                    {
                        return BadRequest(new { success = false, message = $"Domain kustom '{cleanDomain}' sudah digunakan oleh organisasi lain." });
                    }
                }

                var userIds = (model.UserIds != null && model.UserIds.Count > 0)
                    ? model.UserIds.Distinct().ToList()
                    : (model.UserId.HasValue ? new List<long> { model.UserId.Value } : new List<long>());

                var entity = new RestAPISaasBringtimeSolution.Models.Entities.Foundation
                {
                    FoundationTypeId = model.FoundationTypeId,
                    UserId = userIds.Count > 0 ? userIds[0] : model.UserId,
                    Name = model.Name.Trim(),
                    Slug = cleanSlug,
                    Domain = !string.IsNullOrWhiteSpace(model.Domain) ? model.Domain.Trim().ToLower() : null,
                    LogoUrl = model.LogoUrl?.Trim(),
                    BannerUrl = model.BannerUrl?.Trim(),
                    Bio = model.Bio?.Trim(),
                    Address = model.Address?.Trim(),
                    WhatsappNumber = model.WhatsappNumber?.Trim(),
                    LegalPermitNumber = model.LegalPermitNumber?.Trim() ?? "Belum terdaftar",
                    BankName = model.BankName?.Trim(),
                    BankAccountNumber = model.BankAccountNumber?.Trim(),
                    BankAccountHolder = model.BankAccountHolder?.Trim(),
                    IsActive = true
                };

                await _foundationRepo.AddAsync(entity);

                if (userIds.Count > 0)
                {
                    await _foundationRepo.SyncFoundationUsersAsync(entity.Id, userIds);
                }

                return Ok(new { success = true, id = entity.Id, message = "Organisasi mitra berhasil didaftarkan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long id, [FromBody] EditFoundationDto model)
        {
            try
            {
                var entity = await _foundationRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Organisasi mitra tidak ditemukan" });

                if (!string.IsNullOrWhiteSpace(model.Slug))
                {
                    var cleanSlug = model.Slug.Trim().ToLower();
                    var existingBySlug = await _foundationRepo.GetBySlugAsync(cleanSlug);
                    if (existingBySlug != null && existingBySlug.Id != id)
                    {
                        return BadRequest(new { success = false, message = $"Slug URL '{cleanSlug}' sudah digunakan oleh organisasi lain. Silakan pilih slug yang berbeda." });
                    }
                    entity.Slug = cleanSlug;
                }

                if (!string.IsNullOrWhiteSpace(model.Domain))
                {
                    var cleanDomain = model.Domain.Trim().ToLower();
                    var existingByDomain = await _foundationRepo.GetByDomainAsync(cleanDomain);
                    if (existingByDomain != null && existingByDomain.Id != id)
                    {
                        return BadRequest(new { success = false, message = $"Domain kustom '{cleanDomain}' sudah digunakan oleh organisasi lain." });
                    }
                    entity.Domain = cleanDomain;
                }
                else if (model.Domain != null)
                {
                    entity.Domain = null;
                }

                if (model.UserIds != null)
                {
                    var userIds = model.UserIds.Distinct().ToList();
                    if (userIds.Count > 0) entity.UserId = userIds[0];
                    await _foundationRepo.SyncFoundationUsersAsync(entity.Id, userIds);
                }
                else if (model.UserId.HasValue)
                {
                    entity.UserId = model.UserId.Value;
                }

                if (model.FoundationTypeId.HasValue) entity.FoundationTypeId = model.FoundationTypeId.Value;
                if (!string.IsNullOrWhiteSpace(model.Name)) entity.Name = model.Name.Trim();
                if (model.LegalPermitNumber != null) entity.LegalPermitNumber = model.LegalPermitNumber.Trim();
                if (model.LogoUrl != null) entity.LogoUrl = model.LogoUrl.Trim();
                if (model.BannerUrl != null) entity.BannerUrl = model.BannerUrl.Trim();
                if (model.Bio != null) entity.Bio = model.Bio.Trim();
                if (model.Address != null) entity.Address = model.Address.Trim();
                if (model.WhatsappNumber != null) entity.WhatsappNumber = model.WhatsappNumber.Trim();
                if (model.BankName != null) entity.BankName = model.BankName.Trim();
                if (model.BankAccountNumber != null) entity.BankAccountNumber = model.BankAccountNumber.Trim();
                if (model.BankAccountHolder != null) entity.BankAccountHolder = model.BankAccountHolder.Trim();
                entity.IsActive = model.IsActive;

                await _foundationRepo.UpdateAsync(entity);

                return Ok(new { success = true, id = entity.Id, message = "Data organisasi mitra berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.Edit for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPut("{id}/users")]
        [HttpPost("{id}/users")]
        public async Task<IActionResult> UpdateUsers([FromRoute] long id, [FromBody] List<long> userIds)
        {
            try
            {
                var entity = await _foundationRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Yayasan tidak ditemukan" });

                var cleanedIds = (userIds != null) ? userIds.Distinct().ToList() : new List<long>();
                if (cleanedIds.Count > 0)
                {
                    entity.UserId = cleanedIds[0];
                    await _foundationRepo.UpdateAsync(entity);
                }

                await _foundationRepo.SyncFoundationUsersAsync(id, cleanedIds);
                return Ok(new { success = true, message = "Admin Pengelola (PIC) berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.UpdateUsers for Id {Id}", id);
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
                await _foundationRepo.DeleteAsync(targetId.Value);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationsController.Delete for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
