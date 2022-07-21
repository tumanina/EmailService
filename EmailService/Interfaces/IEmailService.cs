using EmailService.Models;

namespace EmailService.Interfaces
{
    public interface IEmailService
    {
        EmailServiceType Type { get; }
        void SendEmail(string email, string subject, string body);
    }
}
