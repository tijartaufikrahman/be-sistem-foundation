using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CampaignRepository> _logger;

        public CampaignRepository(AppDbContext context, ILogger<CampaignRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Campaign>> GetAllAsync()
        {
            try
            {
                var list = await _context.Campaigns.OrderByDescending(c => c.Id).ToListAsync();
                foreach (var c in list)
                {
                    c.CollectedAmount = await GetCollectedAmountAsync(c.Id);
                }
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<IEnumerable<Campaign>> GetByFoundationIdAsync(long foundationId)
        {
            try
            {
                var list = await _context.Campaigns.Where(c => c.FoundationId == foundationId).OrderByDescending(c => c.Id).ToListAsync();
                foreach (var c in list)
                {
                    c.CollectedAmount = await GetCollectedAmountAsync(c.Id);
                }
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.GetByFoundationIdAsync: {FoundationId}", foundationId);
                throw;
            }
        }

        public async Task<Campaign?> GetByIdAsync(long id)
        {
            try
            {
                var c = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id);
                if (c != null)
                {
                    c.CollectedAmount = await GetCollectedAmountAsync(c.Id);
                }
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task<Campaign?> GetBySlugAsync(string slug)
        {
            try
            {
                var c = await _context.Campaigns.FirstOrDefaultAsync(c => c.Slug == slug);
                if (c != null)
                {
                    c.CollectedAmount = await GetCollectedAmountAsync(c.Id);
                }
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.GetBySlugAsync: {Slug}", slug);
                throw;
            }
        }

        public async Task AddAsync(Campaign campaign)
        {
            try
            {
                await _context.Campaigns.AddAsync(campaign);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(Campaign campaign)
        {
            try
            {
                _context.Campaigns.Update(campaign);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.UpdateAsync: {Id}", campaign.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.Campaigns.FindAsync(id);
                if (e != null)
                {
                    _context.Campaigns.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Campaigns.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.CountAsync");
                throw;
            }
        }

        public async Task<decimal> GetCollectedAmountAsync(long campaignId)
        {
            try
            {
                var validStatuses = new[] { "paid", "settled", "success" };
                return await _context.Donations
                    .Where(d => d.CampaignId == campaignId && validStatuses.Contains((d.PaymentStatus ?? "").ToLower()))
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CampaignRepository.GetCollectedAmountAsync: {CampaignId}", campaignId);
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
                _logger.LogError(ex, "Error in CampaignRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
