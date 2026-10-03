using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface INewsPostRepository
    {
        Task<IEnumerable<NewsPost>> GetAllAsync();
        Task<IEnumerable<NewsPost>> GetByFoundationIdAsync(long foundationId);
        Task<NewsPost?> GetByIdAsync(long id);
        Task<NewsPost?> GetBySlugAsync(string slug);
        Task AddAsync(NewsPost newsPost);
        Task UpdateAsync(NewsPost newsPost);
        Task DeleteAsync(long id);
        Task IncrementViewCountAsync(long id);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
