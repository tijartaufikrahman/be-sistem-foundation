using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class MenuRoleRepository : IMenuRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenuRoleRepository> _logger;

        public MenuRoleRepository(AppDbContext context, ILogger<MenuRoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<MenuRole>> GetByRoleIdAsync(long roleId)
        {
            try
            {
                return await _context.MenuRoles
                    .Where(mr => mr.RoleId == roleId)
                    .OrderBy(mr => mr.OrderIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRoleRepository.GetByRoleIdAsync: {RoleId}", roleId);
                throw;
            }
        }

        public async Task<IEnumerable<MenuRole>> GetAllAsync()
        {
            try
            {
                return await _context.MenuRoles
                    .OrderBy(mr => mr.RoleId)
                    .ThenBy(mr => mr.OrderIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRoleRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<MenuRole?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.MenuRoles
                    .FirstOrDefaultAsync(mr => mr.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRoleRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(MenuRole menuRole)
        {
            try
            {
                await _context.MenuRoles.AddAsync(menuRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRoleRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(MenuRole menuRole)
        {
            try
            {
                _context.MenuRoles.Update(menuRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRoleRepository.UpdateAsync: {Id}", menuRole.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.MenuRoles.FindAsync(id);
                if (e != null)
                {
                    _context.MenuRoles.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRoleRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.MenuRoles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRoleRepository.CountAsync");
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
                _logger.LogError(ex, "Error in MenuRoleRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
