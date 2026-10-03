using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId);
        Task<IEnumerable<UserRole>> GetAllAsync();
        Task AddAsync(UserRole userRole);
        Task DeleteAsync(long id);
        Task DeleteByUserAndRoleAsync(long userId, long roleId);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
