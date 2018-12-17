using MultiWalletWorker.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService
{
    public class SendGridEmailService : IEmailService
    {
        private SendGridConfiguration _configuration;

        public SendGridEmailService(SendGridConfiguration configuration)
        {
            _configuration = configuration;
        }

        public EmailServiceType Type
        {
            get { return EmailServiceType.SendGrid; }
        }

        public void SendEmail(string email, string subject, string body)
        {
            var apiKey = _configuration.ApiKey;
            var client = new SendGridClient(apiKey);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress(_configuration.Email, _configuration.Name), new EmailAddress(email), subject, body, string.Empty);

            var response = client.SendEmailAsync(msg).Result;
        }
    }
}
