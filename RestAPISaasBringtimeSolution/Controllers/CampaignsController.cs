using Microsoft.AspNetCore.Mvc;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignRepository _campaignRepo;
        private readonly ILogger<CampaignsController> _logger;

        public CampaignsController(ICampaignRepository campaignRepo, ILogger<CampaignsController> logger)
        {
            _campaignRepo = campaignRepo;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _campaignRepo.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.GetCount");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _campaignRepo.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("foundation/{foundationId}")]
        public async Task<IActionResult> GetByFoundation([FromRoute] long foundationId)
        {
            try
            {
                var result = await _campaignRepo.GetByFoundationIdAsync(foundationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.GetByFoundation for FoundationId {FoundationId}", foundationId);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var result = await _campaignRepo.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug)
        {
            try
            {
                var result = await _campaignRepo.GetBySlugAsync(slug);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.GetBySlug for Slug {Slug}", slug);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCampaignDto model)
        {
            try
            {
                if (model == null) return BadRequest(new { success = false, message = "Data campaign tidak boleh kosong" });

                if (model.FoundationId <= 0) return BadRequest(new { success = false, message = "ID Yayasan (FoundationId) wajib diisi" });
                if (string.IsNullOrWhiteSpace(model.Title)) return BadRequest(new { success = false, message = "Judul campaign wajib diisi" });

                var entity = new RestAPISaasBringtimeSolution.Models.Entities.Campaign
                {
                    FoundationId = model.FoundationId,
                    Title = model.Title,
                    Slug = string.IsNullOrWhiteSpace(model.Slug) ? Guid.NewGuid().ToString("N")[..8] : model.Slug,
                    Category = model.Category ?? "umum",
                    TargetAmount = model.TargetAmount,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    BannerUrl = model.BannerUrl,
                    Description = model.Description ?? string.Empty
                };
                await _campaignRepo.AddAsync(entity);
                return Ok(new { success = true, id = entity.Id, message = "Campaign berhasil dibuat" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id, [FromBody] EditCampaignDto model)
        {
            try
            {
                var targetId = id ?? model?.Id ?? 0;
                var entity = await _campaignRepo.GetByIdAsync(targetId);
                if (entity == null) return NotFound(new { success = false, message = "Campaign tidak ditemukan" });
                
                if (model != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.Title)) entity.Title = model.Title;
                    if (!string.IsNullOrWhiteSpace(model.Slug)) entity.Slug = model.Slug;
                    if (!string.IsNullOrWhiteSpace(model.Category)) entity.Category = model.Category;
                    entity.TargetAmount = model.TargetAmount;
                    entity.StartDate = model.StartDate;
                    entity.EndDate = model.EndDate;
                    if (model.BannerUrl != null) entity.BannerUrl = model.BannerUrl;
                    if (model.Description != null) entity.Description = model.Description;
                    entity.IsActive = model.IsActive;
                }

                await _campaignRepo.UpdateAsync(entity);
                return Ok(new { success = true, id = entity.Id, message = "Campaign berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.Edit for Id {Id}", id);
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
                await _campaignRepo.DeleteAsync(targetId.Value);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignsController.Delete for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
