using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationsController : ControllerBase
    {
        private readonly IDonationRepository _donationRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<DonationsController> _logger;

        public DonationsController(IDonationRepository donationRepo, AppDbContext context, ILogger<DonationsController> logger)
        {
            _donationRepo = donationRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _donationRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (from d in _context.Donations
                                  orderby d.Id descending
                                  join c in _context.Campaigns on d.CampaignId equals c.Id into camp
                                  from c in camp.DefaultIfEmpty()
                                  join f in _context.Foundations on (c != null ? c.FoundationId : 0) equals f.Id into found
                                  from f in found.DefaultIfEmpty()
                                  select new
                                  {
                                      d.Id,
                                      d.CampaignId,
                                      Campaign = c != null ? new { c.Id, c.Title, c.Slug, Foundation = f != null ? new { f.Id, f.Name, f.Slug } : null } : null,
                                      CampaignTitle = c != null ? c.Title : "-",
                                      FoundationName = f != null ? f.Name : "-",
                                      d.UserId,
                                      d.DonorName,
                                      d.DonorEmail,
                                      d.DonorPhone,
                                      d.IsAnonymous,
                                      d.Amount,
                                      d.PlatformTip,
                                      d.TotalAmount,
                                      d.PaymentMethod,
                                      d.PaymentStatus,
                                      d.InvoiceNumber,
                                      d.MidtransOrderId,
                                      d.PaidAt,
                                      d.PrayerMessage,
                                      d.AamiinCount,
                                      d.Created_at
                                  }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("campaign/{campaignId}")]
        public async Task<IActionResult> GetByCampaign([FromRoute] long campaignId)
        {
            try
            {
                var list = await (from d in _context.Donations
                                  where d.CampaignId == campaignId
                                  orderby d.Id descending
                                  join c in _context.Campaigns on d.CampaignId equals c.Id into camp
                                  from c in camp.DefaultIfEmpty()
                                  join f in _context.Foundations on (c != null ? c.FoundationId : 0) equals f.Id into found
                                  from f in found.DefaultIfEmpty()
                                  select new
                                  {
                                      d.Id,
                                      d.CampaignId,
                                      Campaign = c != null ? new { c.Id, c.Title, c.Slug, Foundation = f != null ? new { f.Id, f.Name, f.Slug } : null } : null,
                                      CampaignTitle = c != null ? c.Title : "-",
                                      FoundationName = f != null ? f.Name : "-",
                                      d.UserId,
                                      d.DonorName,
                                      d.DonorEmail,
                                      d.DonorPhone,
                                      d.IsAnonymous,
                                      d.Amount,
                                      d.PlatformTip,
                                      d.TotalAmount,
                                      d.PaymentMethod,
                                      d.PaymentStatus,
                                      d.InvoiceNumber,
                                      d.MidtransOrderId,
                                      d.PaidAt,
                                      d.PrayerMessage,
                                      d.AamiinCount,
                                      d.Created_at
                                  }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.GetByCampaign for CampaignId {CampaignId}", campaignId);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser([FromRoute] long userId, [FromQuery] long? foundationId)
        {
            try
            {
                var query = from d in _context.Donations
                            where d.UserId == userId
                            join c in _context.Campaigns on d.CampaignId equals c.Id into camp
                            from c in camp.DefaultIfEmpty()
                            join f in _context.Foundations on (c != null ? c.FoundationId : 0) equals f.Id into found
                            from f in found.DefaultIfEmpty()
                            select new { d, c, f };

                if (foundationId.HasValue && foundationId.Value > 0)
                {
                    query = query.Where(x => x.c != null && x.c.FoundationId == foundationId.Value);
                }

                var list = await query
                    .OrderByDescending(x => x.d.Id)
                    .Select(x => new
                    {
                        x.d.Id,
                        x.d.CampaignId,
                        Campaign = x.c != null ? new { x.c.Id, x.c.Title, x.c.Slug, Foundation = x.f != null ? new { x.f.Id, x.f.Name, x.f.Slug } : null } : null,
                        CampaignTitle = x.c != null ? x.c.Title : "-",
                        FoundationName = x.f != null ? x.f.Name : "-",
                        x.d.UserId,
                        x.d.DonorName,
                        x.d.DonorEmail,
                        x.d.DonorPhone,
                        x.d.IsAnonymous,
                        x.d.Amount,
                        x.d.PlatformTip,
                        x.d.TotalAmount,
                        x.d.PaymentMethod,
                        x.d.PaymentStatus,
                        x.d.InvoiceNumber,
                        x.d.MidtransOrderId,
                        x.d.PaidAt,
                        x.d.PrayerMessage,
                        x.d.AamiinCount,
                        x.d.Created_at
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.GetByUser for UserId {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var item = await (from d in _context.Donations
                                  where d.Id == id
                                  join c in _context.Campaigns on d.CampaignId equals c.Id into camp
                                  from c in camp.DefaultIfEmpty()
                                  join f in _context.Foundations on (c != null ? c.FoundationId : 0) equals f.Id into found
                                  from f in found.DefaultIfEmpty()
                                  select new
                                  {
                                      d.Id,
                                      d.CampaignId,
                                      Campaign = c != null ? new { c.Id, c.Title, c.Slug, Foundation = f != null ? new { f.Id, f.Name, f.Slug } : null } : null,
                                      CampaignTitle = c != null ? c.Title : "-",
                                      FoundationName = f != null ? f.Name : "-",
                                      d.UserId,
                                      d.DonorName,
                                      d.DonorEmail,
                                      d.DonorPhone,
                                      d.IsAnonymous,
                                      d.Amount,
                                      d.PlatformTip,
                                      d.TotalAmount,
                                      d.PaymentMethod,
                                      d.PaymentStatus,
                                      d.InvoiceNumber,
                                      d.MidtransOrderId,
                                      d.PaidAt,
                                      d.PrayerMessage,
                                      d.AamiinCount,
                                      d.Created_at
                                  }).FirstOrDefaultAsync();

                if (item == null) return NotFound(new { success = false, message = "Donation not found" });
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("invoice/{invoiceNumber}")]
        public async Task<IActionResult> GetByInvoice([FromRoute] string invoiceNumber)
        {
            try
            {
                var item = await (from d in _context.Donations
                                  where d.InvoiceNumber == invoiceNumber
                                  join c in _context.Campaigns on d.CampaignId equals c.Id into camp
                                  from c in camp.DefaultIfEmpty()
                                  join f in _context.Foundations on (c != null ? c.FoundationId : 0) equals f.Id into found
                                  from f in found.DefaultIfEmpty()
                                  select new
                                  {
                                      d.Id,
                                      d.CampaignId,
                                      Campaign = c != null ? new { c.Id, c.Title, c.Slug, Foundation = f != null ? new { f.Id, f.Name, f.Slug } : null } : null,
                                      CampaignTitle = c != null ? c.Title : "-",
                                      FoundationName = f != null ? f.Name : "-",
                                      d.UserId,
                                      d.DonorName,
                                      d.DonorEmail,
                                      d.DonorPhone,
                                      d.IsAnonymous,
                                      d.Amount,
                                      d.PlatformTip,
                                      d.TotalAmount,
                                      d.PaymentMethod,
                                      d.PaymentStatus,
                                      d.InvoiceNumber,
                                      d.MidtransOrderId,
                                      d.PaidAt,
                                      d.PrayerMessage,
                                      d.AamiinCount,
                                      d.Created_at
                                  }).FirstOrDefaultAsync();

                if (item == null) return NotFound(new { success = false, message = "Donation not found" });
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.GetByInvoice for InvoiceNumber {InvoiceNumber}", invoiceNumber);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDonationDto model)
        {
            try
            {
                if (model == null) return BadRequest(new { success = false, message = "Data donasi tidak boleh kosong" });

                if (model.CampaignId <= 0) return BadRequest(new { success = false, message = "ID Campaign wajib diisi" });
                if (model.Amount <= 0) return BadRequest(new { success = false, message = "Nominal donasi harus lebih dari 0" });

                var inv = "INV-" + DateTime.UtcNow.Ticks.ToString();
                var entity = new RestAPISaasBringtimeSolution.Models.Entities.Donation
                {
                    CampaignId = model.CampaignId,
                    UserId = model.UserId,
                    DonorName = string.IsNullOrWhiteSpace(model.DonorName) ? "Hamba Allah" : model.DonorName,
                    DonorEmail = model.DonorEmail,
                    DonorPhone = model.DonorPhone,
                    IsAnonymous = model.IsAnonymous,
                    Amount = model.Amount,
                    PlatformTip = model.PlatformTip,
                    TotalAmount = model.Amount + model.PlatformTip,
                    InvoiceNumber = inv,
                    PrayerMessage = model.PrayerMessage
                };
                await _donationRepo.AddAsync(entity);
                return Ok(new { success = true, id = entity.Id, invoiceNumber = inv, message = "Donasi berhasil dicatat" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}/webhook")]
        public async Task<IActionResult> Webhook([FromRoute] long id, [FromBody] Dictionary<string, object> payload)
        {
            try
            {
                var entity = await _donationRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Donation not found" });

                if (payload.ContainsKey("PaymentStatus"))
                {
                    var newStatus = payload["PaymentStatus"]?.ToString() ?? "pending";
                    var oldStatus = entity.PaymentStatus ?? "pending";
                    entity.PaymentStatus = newStatus;

                    if (newStatus.Equals("paid", StringComparison.OrdinalIgnoreCase) && !oldStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
                    {
                        entity.PaidAt = DateTime.UtcNow;
                        var camp = await _context.Campaigns.FindAsync(entity.CampaignId);
                        if (camp != null)
                        {
                            camp.CollectedAmount += entity.Amount;
                        }
                    }
                }

                await _donationRepo.UpdateAsync(entity);
                await _donationRepo.SaveChangesAsync();
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.Webhook for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}/aamiin")]
        public async Task<IActionResult> IncrementAamiin([FromRoute] long id)
        {
            try
            {
                var entity = await _donationRepo.GetByIdAsync(id);
                if (entity == null) return NotFound(new { success = false, message = "Donation not found" });

                entity.AamiinCount++;
                await _donationRepo.UpdateAsync(entity);
                await _donationRepo.SaveChangesAsync();
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationsController.IncrementAamiin for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
