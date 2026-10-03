using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface ICampaignRepository
    {
        Task<IEnumerable<Campaign>> GetAllAsync();
        Task<IEnumerable<Campaign>> GetByFoundationIdAsync(long foundationId);
        Task<Campaign?> GetByIdAsync(long id);
        Task<Campaign?> GetBySlugAsync(string slug);
        Task AddAsync(Campaign campaign);
        Task UpdateAsync(Campaign campaign);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task<decimal> GetCollectedAmountAsync(long campaignId);
        Task SaveChangesAsync();
    }
}
