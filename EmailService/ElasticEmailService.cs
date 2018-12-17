using ElasticEmailClient;
using MultiWalletWorker.Configuration;
using System.Collections.Generic;

namespace EmailService
{
    public class ElasticEmailService : IEmailService
    {
        private ElasticConfiguration _configuration;

        public ElasticEmailService(ElasticConfiguration configuration)
        {
            _configuration = configuration;
        }

        public EmailServiceType Type
        {
            get { return EmailServiceType.Elastic; }
        }

        public void SendEmail(string email, string subject, string body)
        {
            var apiKey = _configuration.ApiKey;
            string fromEmail = _configuration.Email;
            string fromName = _configuration.Name;

            ApiTypes.EmailSend result = null;
            Api.ApiKey = apiKey;
            result = Api.Email.SendAsync(subject: subject, from: fromEmail, fromName: fromName, msgTo: new List<string> { email }, bodyText: body).Result;

        }
    }
}
