using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class FoundationRepository : IFoundationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FoundationRepository> _logger;

        public FoundationRepository(AppDbContext context, ILogger<FoundationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Foundation>> GetAllAsync()
        {
            try
            {
                return await _context.Foundations.OrderByDescending(f => f.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<Foundation?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Foundations.FirstOrDefaultAsync(f => f.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task<Foundation?> GetBySlugAsync(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug)) return null;
                var clean = slug.Trim().ToLowerInvariant();
                return await _context.Foundations
                    .FirstOrDefaultAsync(f => (f.Slug != null && f.Slug.ToLower() == clean) || (f.Domain != null && f.Domain.ToLower() == clean));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.GetBySlugAsync: {Slug}", slug);
                throw;
            }
        }

        public async Task<Foundation?> GetByDomainAsync(string domain)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(domain)) return null;
                var clean = domain.Trim().ToLowerInvariant();
                return await _context.Foundations
                    .FirstOrDefaultAsync(f => (f.Domain != null && f.Domain.ToLower() == clean) || (f.Slug != null && f.Slug.ToLower() == clean));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.GetByDomainAsync: {Domain}", domain);
                throw;
            }
        }

        public async Task<IEnumerable<Foundation>> GetByUserIdAsync(long userId)
        {
            try
            {
                return await (from fu in _context.FoundationUsers
                              where fu.UserId == userId && fu.IsActive
                              join f in _context.Foundations on fu.FoundationId equals f.Id
                              select f).Distinct().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.GetByUserIdAsync: {UserId}", userId);
                throw;
            }
        }

        public async Task AddAsync(Foundation foundation)
        {
            try
            {
                await _context.Foundations.AddAsync(foundation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(Foundation foundation)
        {
            try
            {
                _context.Foundations.Update(foundation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.UpdateAsync: {Id}", foundation.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.Foundations.FindAsync(id);
                if (e != null)
                {
                    _context.Foundations.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Foundations.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.CountAsync");
                throw;
            }
        }

        public async Task SyncFoundationUsersAsync(long foundationId, List<long> userIds)
        {
            try
            {
                var existing = await _context.FoundationUsers.Where(fu => fu.FoundationId == foundationId).ToListAsync();
                var toRemove = existing.Where(fu => !userIds.Contains(fu.UserId)).ToList();
                if (toRemove.Count > 0)
                {
                    _context.FoundationUsers.RemoveRange(toRemove);
                }

                var existingIds = existing.Select(fu => fu.UserId).ToHashSet();
                foreach (var uid in userIds)
                {
                    if (!existingIds.Contains(uid))
                    {
                        await _context.FoundationUsers.AddAsync(new FoundationUser
                        {
                            FoundationId = foundationId,
                            UserId = uid,
                            IsActive = true,
                            Created_at = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FoundationRepository.SyncFoundationUsersAsync: {FoundationId}", foundationId);
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
                _logger.LogError(ex, "Error in FoundationRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
