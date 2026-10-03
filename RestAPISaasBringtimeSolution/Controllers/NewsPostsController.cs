using Microsoft.AspNetCore.Mvc;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.DTOs;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsPostsController : ControllerBase
    {
        private readonly INewsPostRepository _newsPostRepo;
        private readonly ILogger<NewsPostsController> _logger;

        public NewsPostsController(INewsPostRepository newsPostRepo, ILogger<NewsPostsController> logger)
        {
            _newsPostRepo = newsPostRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _newsPostRepo.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.GetAll");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("foundation/{foundationId}")]
        public async Task<IActionResult> GetByFoundation([FromRoute] long foundationId)
        {
            try
            {
                var result = await _newsPostRepo.GetByFoundationIdAsync(foundationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.GetByFoundation for FoundationId {FoundationId}", foundationId);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            try
            {
                var result = await _newsPostRepo.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.GetById for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug)
        {
            try
            {
                var result = await _newsPostRepo.GetBySlugAsync(slug);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.GetBySlug for Slug {Slug}", slug);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNewsPostDto model)
        {
            try
            {
                if (model == null) return BadRequest(new { success = false, message = "Data berita tidak boleh kosong" });

                if (model.FoundationId <= 0) return BadRequest(new { success = false, message = "ID Yayasan (FoundationId) wajib diisi" });
                if (string.IsNullOrWhiteSpace(model.Title)) return BadRequest(new { success = false, message = "Judul berita wajib diisi" });

                var entity = new RestAPISaasBringtimeSolution.Models.Entities.NewsPost
                {
                    FoundationId = model.FoundationId,
                    Title = model.Title,
                    Slug = string.IsNullOrWhiteSpace(model.Slug) ? Guid.NewGuid().ToString("N")[..8] : model.Slug,
                    ThumbnailUrl = model.ThumbnailUrl,
                    Content = model.Content ?? string.Empty,
                    Category = model.Category ?? "news"
                };
                await _newsPostRepo.AddAsync(entity);
                return Ok(new { success = true, id = entity.Id, message = "Berita berhasil dibuat" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.Create");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id, [FromBody] EditNewsPostDto model)
        {
            try
            {
                var targetId = id ?? model?.Id ?? 0;
                var entity = await _newsPostRepo.GetByIdAsync(targetId);
                if (entity == null) return NotFound(new { success = false, message = "Berita tidak ditemukan" });

                if (model != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.Title)) entity.Title = model.Title;
                    if (!string.IsNullOrWhiteSpace(model.Slug)) entity.Slug = model.Slug;
                    if (model.ThumbnailUrl != null) entity.ThumbnailUrl = model.ThumbnailUrl;
                    if (model.Content != null) entity.Content = model.Content;
                    if (!string.IsNullOrWhiteSpace(model.Category)) entity.Category = model.Category;
                    entity.IsActive = model.IsActive;
                }

                await _newsPostRepo.UpdateAsync(entity);
                return Ok(new { success = true, id = entity.Id, message = "Berita berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.Edit for Id {Id}", id);
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
                await _newsPostRepo.DeleteAsync(targetId.Value);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.Delete for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }

        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementViewCount([FromRoute] long id)
        {
            try
            {
                await _newsPostRepo.IncrementViewCountAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostsController.IncrementViewCount for Id {Id}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
