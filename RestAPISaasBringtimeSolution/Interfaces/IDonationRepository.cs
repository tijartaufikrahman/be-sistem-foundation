using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IDonationRepository
    {
        Task<IEnumerable<Donation>> GetAllAsync();
        Task<IEnumerable<Donation>> GetByCampaignIdAsync(long campaignId);
        Task<IEnumerable<Donation>> GetByUserIdAsync(long userId);
        Task<Donation?> GetByIdAsync(long id);
        Task<Donation?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task AddAsync(Donation donation);
        Task UpdateAsync(Donation donation);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
