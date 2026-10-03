using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class MenuGroupRepository : IMenuGroupRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenuGroupRepository> _logger;

        public MenuGroupRepository(AppDbContext context, ILogger<MenuGroupRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<MenuGroup>> GetAllAsync()
        {
            try
            {
                return await _context.MenuGroups
                    .OrderBy(mg => mg.GroupId)
                    .ThenBy(mg => mg.OrderIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<IEnumerable<MenuGroup>> GetByGroupIdAsync(long groupId)
        {
            try
            {
                return await _context.MenuGroups
                    .Where(mg => mg.GroupId == groupId && mg.IsActive)
                    .OrderBy(mg => mg.OrderIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupRepository.GetByGroupIdAsync: {GroupId}", groupId);
                throw;
            }
        }

        public async Task<MenuGroup?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.MenuGroups
                    .FirstOrDefaultAsync(mg => mg.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task<MenuGroup> AddAsync(MenuGroup menuGroup)
        {
            try
            {
                await _context.MenuGroups.AddAsync(menuGroup);
                await _context.SaveChangesAsync();
                return menuGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(MenuGroup menuGroup)
        {
            try
            {
                _context.MenuGroups.Update(menuGroup);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupRepository.UpdateAsync: {Id}", menuGroup.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var entity = await _context.MenuGroups.FindAsync(id);
                if (entity != null)
                {
                    _context.MenuGroups.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.MenuGroups.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuGroupRepository.CountAsync");
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
                _logger.LogError(ex, "Error in MenuGroupRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
