using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IMenuGroupRepository
    {
        Task<IEnumerable<MenuGroup>> GetAllAsync();
        Task<IEnumerable<MenuGroup>> GetByGroupIdAsync(long groupId);
        Task<MenuGroup?> GetByIdAsync(long id);
        Task<MenuGroup> AddAsync(MenuGroup menuGroup);
        Task UpdateAsync(MenuGroup menuGroup);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
