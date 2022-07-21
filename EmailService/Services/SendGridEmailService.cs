using EmailService.Configuration;
using EmailService.Interfaces;
using EmailService.Models;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService.Services
{
    public class SendGridEmailService : IEmailService
    {
        private SendGridConfiguration _configuration;

        public SendGridEmailService(IOptions<SendGridConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        public EmailProvider Provider
        {
            get { return EmailProvider.SendGrid; }
        }

        public void SendEmail(string email, string subject, string body)
        {
            var apiKey = _configuration.ApiKey;
            var client = new SendGridClient(apiKey);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress(_configuration.Email, _configuration.Name), new EmailAddress(email), subject, body, string.Empty);

            var response = client.SendEmailAsync(msg).Result;
        }

        public void SendEmail(string email, string templateId, Dictionary<string, string> parameters)
        {
            var apiKey = _configuration.ApiKey;
            var client = new SendGridClient(apiKey);

            var msg = MailHelper.CreateSingleTemplateEmail(new EmailAddress(_configuration.Email, _configuration.Name), new EmailAddress(email), templateId, parameters);

            var response = client.SendEmailAsync(msg).Result;
        }
    }
}
