using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Repositories
{
    public class DonationRepository : IDonationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DonationRepository> _logger;

        public DonationRepository(AppDbContext context, ILogger<DonationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Donation>> GetAllAsync()
        {
            try
            {
                return await _context.Donations.OrderByDescending(d => d.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.GetAllAsync");
                throw;
            }
        }

        public async Task<IEnumerable<Donation>> GetByCampaignIdAsync(long campaignId)
        {
            try
            {
                return await _context.Donations.Where(d => d.CampaignId == campaignId).OrderByDescending(d => d.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.GetByCampaignIdAsync: {CampaignId}", campaignId);
                throw;
            }
        }

        public async Task<IEnumerable<Donation>> GetByUserIdAsync(long userId)
        {
            try
            {
                return await _context.Donations.Where(d => d.UserId == userId).OrderByDescending(d => d.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.GetByUserIdAsync: {UserId}", userId);
                throw;
            }
        }

        public async Task<Donation?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Donations.FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.GetByIdAsync: {Id}", id);
                throw;
            }
        }

        public async Task<Donation?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            try
            {
                return await _context.Donations.FirstOrDefaultAsync(d => d.InvoiceNumber == invoiceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.GetByInvoiceNumberAsync: {InvoiceNumber}", invoiceNumber);
                throw;
            }
        }

        public async Task AddAsync(Donation donation)
        {
            try
            {
                await _context.Donations.AddAsync(donation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(Donation donation)
        {
            try
            {
                _context.Donations.Update(donation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.UpdateAsync: {Id}", donation.Id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Donations.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DonationRepository.CountAsync");
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
                _logger.LogError(ex, "Error in DonationRepository.SaveChangesAsync");
                throw;
            }
        }
    }
}
