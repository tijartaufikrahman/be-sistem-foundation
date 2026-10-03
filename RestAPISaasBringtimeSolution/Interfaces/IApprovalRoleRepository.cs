using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IApprovalRoleRepository
    {
        Task<IEnumerable<ApprovalRole>> GetByApprovalIdAsync(long approvalId);
        Task<IEnumerable<ApprovalRole>> GetAllAsync();
        Task<ApprovalRole?> GetByIdAsync(long id);
        Task AddAsync(ApprovalRole approvalRole);
        Task UpdateAsync(ApprovalRole approvalRole);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
