using ElasticEmailClient;
using EmailService.Configuration;
using EmailService.Interfaces;
using EmailService.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace EmailService.Services
{
    public class ElasticEmailService : IEmailService
    {
        private ElasticConfiguration _configuration;

        public ElasticEmailService(IOptions<ElasticConfiguration> configuration)
        {
            _configuration = configuration.Value;
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
