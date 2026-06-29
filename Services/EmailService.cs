using Microsoft.Extensions.Options;
using ShopManagementAPI.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ShopManagement.Infrastructure.Templates;

namespace ShopManagementAPI.Services
{
    public class EmailService
    {
        // Cấu hình otp,SMTP được lấy từ appsettings.json
        private readonly EmailSettings _emailSettings;
        private readonly OtpSettings _otpSettings;

        public EmailService(IOptions<EmailSettings> emailSettings,IOptions<OtpSettings> otpSettings)
        {
            _emailSettings = emailSettings.Value;
            _otpSettings = otpSettings.Value;
        }

        // Tạo nội dung email OTP và gửi email
        public async Task SendOtpAsync(string email,string subject,string message,string otp)
        {
            var body = EmailTemplateBuilder.BuildOtpTemplate(
                subject,
                message,
                otp,
                _otpSettings.ExpiredMinutes);

            await SendEmailAsync(email, subject, body);
        }

        // Gửi email qua SMTP
        private async Task SendEmailAsync(string email,string subject,string body)
        {
            var message = new MimeMessage();

            // Thiết lập người gửi, người nhận và nội dung email
            message.From.Add(new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            // Kết nối, xác thực và gửi email
            await smtp.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.Port,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _emailSettings.SenderEmail,
                _emailSettings.Password);

            await smtp.SendAsync(message);

            // Ngắt kết nối
            await smtp.DisconnectAsync(true);
        }
    }
}
