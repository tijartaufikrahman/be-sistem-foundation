using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IAuthRepository
    {
        Task<(bool Success, string Message, User? User, Foundation? TargetFoundation)> LoginAsync(string email, string password, string? foundationSlug = null, long? foundationId = null);
        Task<(bool Success, string Message, User? User)> RegisterDonorAsync(string name, string email, string password, string? phoneNumber, string? foundationSlug = null, long? foundationId = null);
        Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl);
        Task<(bool Success, string Message)> SendForgotPasswordOtpAsync(string email, string? foundationSlug = null);
        Task<(bool Success, string Message)> VerifyOtpAsync(string email, string otp, string? foundationSlug = null);
        Task<(bool Success, string Message)> ResetPasswordWithOtpAsync(string email, string otp, string newPassword, string? foundationSlug = null);
        Task<bool> ValidateResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
