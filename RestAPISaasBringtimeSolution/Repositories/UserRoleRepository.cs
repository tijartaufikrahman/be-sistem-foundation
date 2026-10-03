using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRoleRepository> _logger;

        public UserRoleRepository(AppDbContext context, ILogger<UserRoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId)
        {
            try
            {
                return await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRoleRepository.GetByUserIdAsync: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserRole>> GetAllAsync()
        {
            try
            {
                return await _context.UserRoles.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRoleRepository.GetAllAsync");
                throw;
            }
        }

        public async Task AddAsync(UserRole userRole)
        {
            try
            {
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRoleRepository.AddAsync");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.UserRoles.FindAsync(id);
                if (e != null)
                {
                    _context.UserRoles.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRoleRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task DeleteByUserAndRoleAsync(long userId, long roleId)
        {
            try
            {
                var e = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
                if (e != null)
                {
                    _context.UserRoles.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRoleRepository.DeleteByUserAndRoleAsync: User {UserId}, Role {RoleId}", userId, roleId);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.UserRoles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRoleRepository.CountAsync");
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
                _logger.LogError(ex, "Error in UserRoleRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
