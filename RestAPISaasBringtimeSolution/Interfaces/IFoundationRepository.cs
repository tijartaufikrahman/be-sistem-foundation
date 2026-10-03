using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IFoundationRepository
    {
        Task<IEnumerable<Foundation>> GetAllAsync();
        Task<Foundation?> GetByIdAsync(long id);
        Task<Foundation?> GetBySlugAsync(string slug);
        Task<Foundation?> GetByDomainAsync(string domain);
        Task<IEnumerable<Foundation>> GetByUserIdAsync(long userId);
        Task AddAsync(Foundation foundation);
        Task UpdateAsync(Foundation foundation);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task SyncFoundationUsersAsync(long foundationId, List<long> userIds);
        Task SaveChangesAsync();
    }
}
