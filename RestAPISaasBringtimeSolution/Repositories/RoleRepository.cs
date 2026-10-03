using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(AppDbContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            try
            {
                return await _context.Roles.OrderBy(r => r.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoleRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<Role?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Roles.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoleRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(Role role)
        {
            try
            {
                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoleRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(Role role)
        {
            try
            {
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoleRepository.UpdateAsync: {Id}", role.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.Roles.FindAsync(id);
                if (e != null)
                {
                    _context.Roles.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoleRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<bool> HasUsersAsync(long roleId)
        {
            try
            {
                return await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoleRepository.HasUsersAsync: {RoleId}", roleId);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Roles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoleRepository.CountAsync");
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
                _logger.LogError(ex, "Error in RoleRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
