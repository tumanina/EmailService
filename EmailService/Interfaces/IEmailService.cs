using EmailService.Models;

namespace EmailService.Interfaces
{
    public interface IEmailService
    {
        EmailProvider Provider { get; }
        void SendEmail(string email, string subject, string body);
        void SendEmail(string email, string templateId, Dictionary<string, string> parameters);
    }
}
