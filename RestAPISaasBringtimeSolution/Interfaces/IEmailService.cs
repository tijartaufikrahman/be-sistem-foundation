namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Mengirim email dengan subject, konten body (HTML / Text), dan penerima dinamis.
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, string? toName = null);

        /// <summary>
        /// Mengirim email reset password dengan template HTML responsif dan link token dinamis.
        /// </summary>
        Task<bool> SendResetPasswordEmailAsync(string toEmail, string toName, string resetLink, string? foundationName = null);

        /// <summary>
        /// Mengirim email reset password berupa kode OTP 6-digit dinamis dengan template bergaya profesional.
        /// </summary>
        Task<bool> SendResetPasswordOtpEmailAsync(string toEmail, string toName, string otpCode, string? foundationName = null);

        /// <summary>
        /// Mengirim email notifikasi dinamis untuk verifikasi atau informasi sistem penting.
        /// </summary>
        Task<bool> SendNotificationEmailAsync(string toEmail, string toName, string title, string message, string? actionUrl = null, string? actionText = null);
    }
}
