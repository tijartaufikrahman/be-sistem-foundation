using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllAsync();
        Task<Group?> GetByIdAsync(long id);
        Task<Group> AddAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
