using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApprovalRepository> _logger;

        public ApprovalRepository(AppDbContext context, ILogger<ApprovalRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Approval>> GetAllAsync()
        {
            try
            {
                return await _context.Approvals.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<Approval?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Approvals.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(Approval approval)
        {
            try
            {
                await _context.Approvals.AddAsync(approval);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(Approval approval)
        {
            try
            {
                _context.Approvals.Update(approval);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRepository.UpdateAsync: {Id}", approval.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.Approvals.FindAsync(id);
                if (e != null)
                {
                    _context.Approvals.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Approvals.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalRepository.CountAsync");
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
                _logger.LogError(ex, "Error in ApprovalRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
