using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RestAPISaasBringtimeSolution.Interfaces;

namespace RestAPISaasBringtimeSolution.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, string? toName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    _logger.LogWarning("SendEmailAsync dibatalkan karena email tujuan kosong.");
                    return false;
                }

                // Baca konfigurasi dari EmailSettings (sesuai aturan_jwt_email.txt) dengan fallback ke Smtp
                var host = _config["EmailSettings:Host"] ?? _config["Smtp:Host"] ?? "smtp.gmail.com";
                var portStr = _config["EmailSettings:Port"] ?? _config["Smtp:Port"] ?? "587";
                var port = int.TryParse(portStr, out var p) ? p : 587;
                var senderEmail = _config["EmailSettings:Email"] ?? _config["Smtp:Username"] ?? _config["Smtp:Email"] ?? "";
                var senderPassword = _config["EmailSettings:Password"] ?? _config["Smtp:Password"] ?? "";
                var fromName = _config["EmailSettings:FromName"] ?? _config["Smtp:FromName"] ?? "Bringtime Solution";

                if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
                {
                    _logger.LogError("Konfigurasi SMTP/EmailSettings belum lengkap (email atau password kosong).");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, senderEmail));
                message.To.Add(new MailboxAddress(string.IsNullOrWhiteSpace(toName) ? toEmail : toName, toEmail.Trim()));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                // Port 465 biasanya SSL murni, port 587 menggunakan StartTls
                var secureOption = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                // Timeout 15 detik agar responsif
                client.Timeout = 15000;

                await client.ConnectAsync(host, port, secureOption);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email berhasil dikirim ke {ToEmail} dengan subject '{Subject}'", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengirim email ke {ToEmail}. Pesan error: {ErrorMessage}", toEmail, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string toName, string resetLink, string? foundationName = null)
        {
            var brandName = !string.IsNullOrWhiteSpace(foundationName) ? foundationName : "Bringtime Solution";
            var subject = $"Reset Password Akun - {brandName}";

            var htmlContent = $@"
<!DOCTYPE html>
<html lang='id'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{subject}</title>
    <style>
        body {{ font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }}
        .wrapper {{ max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 20px rgba(15, 23, 42, 0.06); }}
        .header {{ background: linear-gradient(135deg, #0f766e 0%, #0d9488 100%); padding: 36px 30px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 22px; font-weight: 700; letter-spacing: -0.3px; }}
        .header p {{ margin: 6px 0 0; font-size: 13px; color: #ccfbf1; font-weight: 500; }}
        .body {{ padding: 32px; }}
        .greeting {{ font-size: 15px; font-weight: 600; color: #0f172a; margin-bottom: 12px; }}
        .desc {{ font-size: 14px; color: #475569; line-height: 1.65; margin-bottom: 24px; }}
        .btn-container {{ text-align: center; margin: 28px 0; }}
        .btn {{ display: inline-block; padding: 13px 32px; background: linear-gradient(135deg, #0f766e, #0d9488); color: #ffffff !important; text-decoration: none; border-radius: 10px; font-weight: 600; font-size: 14px; box-shadow: 0 4px 12px rgba(13, 148, 136, 0.25); }}
        .warning-box {{ background: #fefce8; border: 1px solid #fef08a; border-radius: 10px; padding: 14px 16px; font-size: 12.5px; color: #854d0e; line-height: 1.5; margin-top: 24px; }}
        .link-fallback {{ margin-top: 20px; font-size: 12px; color: #64748b; line-height: 1.6; word-break: break-all; }}
        .link-fallback a {{ color: #0f766e; text-decoration: underline; }}
        .footer {{ background: #f8fafc; padding: 18px 32px; text-align: center; font-size: 11.5px; color: #94a3b8; border-top: 1px solid #f1f5f9; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='header'>
            <h1>🔐 {brandName}</h1>
            <p>Layanan Pemulihan Kata Sandi Akun</p>
        </div>
        <div class='body'>
            <div class='greeting'>Halo {toName},</div>
            <div class='desc'>
                Kami menerima permintaan untuk mereset kata sandi akun Anda di <strong>{brandName}</strong>. Jika Anda memang melakukan permintaan ini, silakan klik tombol di bawah untuk membuat kata sandi baru Anda:
            </div>
            <div class='btn-container'>
                <a href='{resetLink}' target='_blank' class='btn'>Atur Ulang Kata Sandi</a>
            </div>
            <div class='warning-box'>
                ⚠️ <strong>Pemberitahuan Keamanan:</strong> Link reset password ini hanya berlaku selama <strong>1 jam</strong>. Jika Anda tidak merasa meminta reset kata sandi, mohon abaikan pesan ini dan akun Anda akan tetap aman.
            </div>
            <div class='link-fallback'>
                Jika tombol di atas tidak berfungsi, Anda dapat menyalin dan membuka URL berikut di browser:<br>
                <a href='{resetLink}'>{resetLink}</a>
            </div>
        </div>
        <div class='footer'>
            &copy; 2026 {brandName}. Seluruh hak cipta dilindungi undang-undang.<br>
            Email otomatis dari sistem &mdash; mohon jangan membalas email ini.
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlContent, isHtml: true, toName: toName);
        }

        public async Task<bool> SendResetPasswordOtpEmailAsync(string toEmail, string toName, string otpCode, string? foundationName = null)
        {
            var brandName = !string.IsNullOrWhiteSpace(foundationName) ? foundationName : "Bringtime Solution";
            var subject = $"Kode OTP Reset Password: {otpCode} - {brandName}";

            var htmlContent = $@"
<!DOCTYPE html>
<html lang='id'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{subject}</title>
    <style>
        body {{ font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, Roboto, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }}
        .wrapper {{ max-width: 520px; margin: 0 auto; background: #ffffff; border-radius: 20px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 10px 40px rgba(15, 23, 42, 0.08); }}
        .header {{ background: linear-gradient(135deg, #0f766e 0%, #0d9488 100%); padding: 36px 30px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 22px; font-weight: 700; }}
        .header p {{ margin: 6px 0 0; font-size: 13px; color: #ccfbf1; font-weight: 500; }}
        .body {{ padding: 32px 30px; }}
        .greeting {{ font-size: 15px; font-weight: 600; color: #0f172a; margin-bottom: 12px; }}
        .desc {{ font-size: 14px; color: #475569; line-height: 1.65; margin-bottom: 24px; }}
        .otp-container {{ text-align: center; margin: 28px 0; }}
        .otp-box {{ display: inline-block; background: #f0fdfa; border: 2px dashed #0d9488; border-radius: 14px; padding: 18px 40px; box-shadow: 0 4px 14px rgba(13,148,136,0.12); }}
        .otp-label {{ font-size: 11px; font-weight: 700; color: #0d9488; letter-spacing: 2px; text-transform: uppercase; margin-bottom: 6px; }}
        .otp-code {{ font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #0f766e; font-family: Consolas, 'Courier New', monospace; }}
        .warning-box {{ background: #fefce8; border: 1px solid #fef08a; border-radius: 12px; padding: 14px 16px; font-size: 12.5px; color: #854d0e; line-height: 1.55; margin-top: 24px; }}
        .security-note {{ margin-top: 18px; font-size: 12.5px; color: #64748b; line-height: 1.6; }}
        .footer {{ background: #f8fafc; padding: 18px 30px; text-align: center; font-size: 11.5px; color: #94a3b8; border-top: 1px solid #f1f5f9; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='header'>
            <h1>🔐 {brandName}</h1>
            <p>Verifikasi Keamanan Reset Password</p>
        </div>
        <div class='body'>
            <div class='greeting'>Halo {toName},</div>
            <div class='desc'>
                Kami menerima permintaan reset password untuk akun Anda di <strong>{brandName}</strong>. Masukkan kode One-Time Password (OTP) berikut untuk melanjutkan:
            </div>
            <div class='otp-container'>
                <div class='otp-box'>
                    <div class='otp-label'>Kode OTP Reset Password</div>
                    <div class='otp-code'>{otpCode}</div>
                </div>
            </div>
            <div class='warning-box'>
                ⏱️ <strong>Masa Berlaku:</strong> Kode OTP ini hanya berlaku selama <strong>15 menit</strong>. Jangan berikan kode ini kepada siapapun demi menjaga keamanan akun Anda.
            </div>
            <div class='security-note'>
                Jika Anda tidak merasa meminta kode ini, mohon abaikan email ini. Password Anda tidak akan berubah tanpa konfirmasi kode OTP di atas.
            </div>
        </div>
        <div class='footer'>
            &copy; 2026 {brandName}. Seluruh hak cipta dilindungi undang-undang.<br>
            Email otomatis dari sistem &mdash; mohon jangan membalas email ini.
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlContent, isHtml: true, toName: toName);
        }

        public async Task<bool> SendNotificationEmailAsync(string toEmail, string toName, string title, string message, string? actionUrl = null, string? actionText = null)
        {
            var brandName = "Bringtime Solution";
            var subject = $"{title} - {brandName}";

            var actionButtonHtml = "";
            if (!string.IsNullOrWhiteSpace(actionUrl) && !string.IsNullOrWhiteSpace(actionText))
            {
                actionButtonHtml = $@"
                <div class='btn-container'>
                    <a href='{actionUrl}' target='_blank' class='btn'>{actionText}</a>
                </div>";
            }

            var htmlContent = $@"
<!DOCTYPE html>
<html lang='id'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{subject}</title>
    <style>
        body {{ font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }}
        .wrapper {{ max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 20px rgba(15, 23, 42, 0.06); }}
        .header {{ background: linear-gradient(135deg, #0f766e 0%, #0d9488 100%); padding: 32px 30px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 20px; font-weight: 700; }}
        .body {{ padding: 32px; }}
        .greeting {{ font-size: 15px; font-weight: 600; color: #0f172a; margin-bottom: 12px; }}
        .desc {{ font-size: 14px; color: #475569; line-height: 1.65; margin-bottom: 20px; }}
        .btn-container {{ text-align: center; margin: 24px 0; }}
        .btn {{ display: inline-block; padding: 12px 28px; background: linear-gradient(135deg, #0f766e, #0d9488); color: #ffffff !important; text-decoration: none; border-radius: 10px; font-weight: 600; font-size: 14px; }}
        .footer {{ background: #f8fafc; padding: 18px 32px; text-align: center; font-size: 11.5px; color: #94a3b8; border-top: 1px solid #f1f5f9; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='header'>
            <h1>{brandName}</h1>
        </div>
        <div class='body'>
            <div class='greeting'>Halo {toName},</div>
            <div class='desc'>{message}</div>
            {actionButtonHtml}
        </div>
        <div class='footer'>
            &copy; 2026 {brandName}. Email notifikasi otomatis sistem.
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlContent, isHtml: true, toName: toName);
        }
    }
}
