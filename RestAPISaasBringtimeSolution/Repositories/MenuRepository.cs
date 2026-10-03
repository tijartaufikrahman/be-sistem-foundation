using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenuRepository> _logger;

        public MenuRepository(AppDbContext context, ILogger<MenuRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Menu>> GetAllAsync()
        {
            try
            {
                return await _context.Menus.OrderBy(m => m.OrderIndex).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<Menu?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Menus.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(Menu menu)
        {
            try
            {
                await _context.Menus.AddAsync(menu);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(Menu menu)
        {
            try
            {
                _context.Menus.Update(menu);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRepository.UpdateAsync: {Id}", menu.Id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var e = await _context.Menus.FindAsync(id);
                if (e != null)
                {
                    _context.Menus.Remove(e);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRepository.DeleteAsync: {Id}", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Menus.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MenuRepository.CountAsync");
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
                _logger.LogError(ex, "Error in MenuRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
