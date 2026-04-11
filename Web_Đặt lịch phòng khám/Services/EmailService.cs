using MailKit.Net.Smtp;
using MimeKit;
using Web_Đặt_lịch_phòng_khám.Services;

namespace Web_Đặt_lịch_phòng_khám.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Phòng Khám", "your-email@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = message };

            // Sử dụng đường dẫn đầy đủ để tránh lỗi CS0104
            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                // Nhớ thay App Password vào đây
                await client.AuthenticateAsync("your-email@gmail.com", "your-app-password");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                throw new Exception("Lỗi gửi mail: " + ex.Message);
            }
        }
    }
}