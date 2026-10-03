namespace RestAPISaasBringtimeSolution.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class ApiResponse : ApiResponse<object>
    {
    }

    public class DeleteRequestDto
    {
        public long Id { get; set; }
    }

    public class DashboardStatsDto
    {
        public long TotalUsers { get; set; }
        public long TotalRoles { get; set; }
        public long TotalMenus { get; set; }
        public long TotalFoundations { get; set; }
        public long TotalCampaigns { get; set; }
        public long TotalDonations { get; set; }
    }
}
