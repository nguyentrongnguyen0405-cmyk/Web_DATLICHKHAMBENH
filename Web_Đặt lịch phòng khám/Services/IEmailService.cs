using MimeKit;
using MailKit.Net.Smtp;

namespace Web_Đặt_lịch_phòng_khám.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}