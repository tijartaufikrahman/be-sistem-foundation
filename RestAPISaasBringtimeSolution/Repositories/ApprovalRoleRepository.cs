using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class ApprovalRoleRepository : IApprovalRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApprovalRoleRepository> _logger;

        public ApprovalRoleRepository(AppDbContext context, ILogger<ApprovalRoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ApprovalRole>> GetByApprovalIdAsync(long approvalId)
        {
            try
            {
                return await _context.ApprovalRoles.Where(ar => ar.ApprovalId == approvalId).OrderBy(ar => ar.StepLevel).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.GetByApprovalIdAsync: {ApprovalId}", approvalId);
                throw;
            }
        }

        public async Task<IEnumerable<ApprovalRole>> GetAllAsync()
        {
            try
            {
                return await _context.ApprovalRoles.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<ApprovalRole?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.ApprovalRoles.FirstOrDefaultAsync(ar => ar.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(ApprovalRole approvalRole)
        {
            try
            {
                await _context.ApprovalRoles.AddAsync(approvalRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(ApprovalRole approvalRole)
        {
            try
            {
                _context.ApprovalRoles.Update(approvalRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.UpdateAsync: {Id}", approvalRole.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.ApprovalRoles.FindAsync(id);
                if (e != null)
                {
                    _context.ApprovalRoles.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.ApprovalRoles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.CountAsync");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRoleRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
