using Microsoft.AspNetCore.Mvc;
using RestAPISaasBringtimeSolution.Interfaces;

namespace RestAPISaasBringtimeSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly IFoundationRepository _foundationRepo;
        private readonly ICampaignRepository _campaignRepo;
        private readonly IDonationRepository _donationRepo;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IMenuRepository menuRepo,
            IFoundationRepository foundationRepo,
            ICampaignRepository campaignRepo,
            IDonationRepository donationRepo,
            ILogger<DashboardController> logger)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _menuRepo = menuRepo;
            _foundationRepo = foundationRepo;
            _campaignRepo = campaignRepo;
            _donationRepo = donationRepo;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var allDonations = await _donationRepo.GetAllAsync();
                var allCampaigns = await _campaignRepo.GetAllAsync();
                var allFoundations = await _foundationRepo.GetAllAsync();

                var stats = new
                {
                    TotalUsers = await _userRepo.CountAsync(),
                    TotalRoles = await _roleRepo.CountAsync(),
                    TotalMenus = await _menuRepo.CountAsync(),
                    TotalFoundations = allFoundations.Count(),
                    ActiveFoundations = allFoundations.Count(f => f.IsActive),
                    TotalCampaigns = allCampaigns.Count(),
                    ActiveCampaigns = allCampaigns.Count(c => c.IsActive),
                    TotalDonations = allDonations.Count(),
                    PaidDonations = allDonations.Count(d => {
                        var st = (d.PaymentStatus ?? "").ToLower();
                        return st == "paid" || st == "settled" || st == "success";
                    }),
                    TotalDonationAmount = allDonations.Where(d => {
                        var st = (d.PaymentStatus ?? "").ToLower();
                        return st == "paid" || st == "settled" || st == "success";
                    }).Sum(d => d.Amount)
                };

                return Ok(new
                {
                    success = true,
                    data = stats,
                    totalUsers = stats.TotalUsers,
                    totalRoles = stats.TotalRoles,
                    totalMenus = stats.TotalMenus,
                    totalFoundations = stats.TotalFoundations,
                    activeFoundations = stats.ActiveFoundations,
                    totalCampaigns = stats.TotalCampaigns,
                    activeCampaigns = stats.ActiveCampaigns,
                    totalDonations = stats.TotalDonations,
                    paidDonations = stats.PaidDonations,
                    totalDonationAmount = stats.TotalDonationAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DashboardController.GetStats");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal pada server: " + ex.Message });
            }
        }
    }
}
