using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IApprovalRepository
    {
        Task<IEnumerable<Approval>> GetAllAsync();
        Task<Approval?> GetByIdAsync(long id);
        Task AddAsync(Approval approval);
        Task UpdateAsync(Approval approval);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
