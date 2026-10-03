using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GroupRepository> _logger;

        public GroupRepository(AppDbContext context, ILogger<GroupRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            try
            {
                return await _context.Groups
                    .OrderBy(g => g.OrderIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<Group?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Groups
                    .FirstOrDefaultAsync(g => g.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task<Group> AddAsync(Group group)
        {
            try
            {
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();
                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(Group group)
        {
            try
            {
                _context.Groups.Update(group);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupRepository.UpdateAsync: {Id}", group.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var entity = await _context.Groups.FindAsync(id);
                if (entity != null)
                {
                    // Bersihkan asosiasi menu di dalam grup ini (menu_groups)
                    var relatedMenuGroups = await _context.MenuGroups.Where(mg => mg.GroupId == id).ToListAsync();
                    if (relatedMenuGroups.Any())
                    {
                        _context.MenuGroups.RemoveRange(relatedMenuGroups);
                    }

                    // Bersihkan asosiasi hak akses peran yang mereferensikan grup ini (menu_roles)
                    var relatedMenuRoles = await _context.MenuRoles.Where(mr => mr.GroupId == id).ToListAsync();
                    if (relatedMenuRoles.Any())
                    {
                        _context.MenuRoles.RemoveRange(relatedMenuRoles);
                    }

                    _context.Groups.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Groups.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GroupRepository.CountAsync");
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
                _logger.LogError(ex, "Error in GroupRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
