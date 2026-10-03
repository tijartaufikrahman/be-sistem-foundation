using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.GetByEmailAsync: {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task<User?> GetByResetTokenAsync(string token)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.GetByResetTokenAsync");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.GetAllAsync");
                throw;
            }
        }

        public async Task AddAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.UpdateAsync: {Id}", user.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.Users.FindAsync(id);
                if (e != null)
                {
                    _context.Users.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRepository.CountAsync");
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
                _logger.LogError(ex, "Error in UserRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
