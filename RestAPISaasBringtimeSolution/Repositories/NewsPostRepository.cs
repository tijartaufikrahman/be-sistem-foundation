using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class NewsPostRepository : INewsPostRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NewsPostRepository> _logger;

        public NewsPostRepository(AppDbContext context, ILogger<NewsPostRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<NewsPost>> GetAllAsync()
        {
            try
            {
                return await _context.NewsPosts.OrderByDescending(n => n.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<IEnumerable<NewsPost>> GetByFoundationIdAsync(long foundationId)
        {
            try
            {
                return await _context.NewsPosts.Where(n => n.FoundationId == foundationId).OrderByDescending(n => n.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.GetByFoundationIdAsync: {FoundationId}", foundationId);
                throw;
            }
        }

        public async Task<NewsPost?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.NewsPosts.FirstOrDefaultAsync(n => n.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task<NewsPost?> GetBySlugAsync(string slug)
        {
            try
            {
                return await _context.NewsPosts.FirstOrDefaultAsync(n => n.Slug == slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.GetBySlugAsync: {Slug}", slug);
                throw;
            }
        }

        public async Task AddAsync(NewsPost newsPost)
        {
            try
            {
                await _context.NewsPosts.AddAsync(newsPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(NewsPost newsPost)
        {
            try
            {
                _context.NewsPosts.Update(newsPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.UpdateAsync: {Id}", newsPost.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.NewsPosts.FindAsync(id);
                if (e != null)
                {
                    _context.NewsPosts.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task IncrementViewCountAsync(long id)
        {
            try
            {
                var p = await _context.NewsPosts.FindAsync(id);
                if (p != null)
                {
                    p.ViewCount++;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.IncrementViewCountAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.NewsPosts.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.CountAsync");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NewsPostRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
