using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IMenuRepository
    {
        Task<IEnumerable<Menu>> GetAllAsync();
        Task<Menu?> GetByIdAsync(long id);
        Task AddAsync(Menu menu);
        Task UpdateAsync(Menu menu);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
