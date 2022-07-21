using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using EmailService.Configuration;
using EmailService.Interfaces;
using Microsoft.Extensions.Options;
using EmailService.Models;

namespace EmailService.Services
{
    public class SendInBlueEmailService : IEmailService
    {
        private SendInBlueConfiguration _configuration;

        public SendInBlueEmailService(IOptions<SendInBlueConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        public EmailServiceType Type
        {
            get { return EmailServiceType.SendInBlue; }
        }

        public void SendEmail(string email, string subject, string body)
        {
            sib_api_v3_sdk.Client.Configuration.Default.ApiKey.Add("api-key", _configuration.ApiKey);

            var apiInstance = new TransactionalEmailsApi();

            var emailMessage = new SendSmtpEmail(to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: email) },
            sender: new SendSmtpEmailSender { Name = _configuration.Name, Email = _configuration.Email },
            subject: subject, textContent: body);

            var result = apiInstance.SendTransacEmail(emailMessage);
        }
    }
}
