using EmailService.Interfaces;
using EmailService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers
{
    [ApiController]
    [Route("api/emails")]
    public class EmailSendingController : ControllerBase
    {
        private readonly ILogger<EmailSendingController> _logger;
        private readonly IEnumerable<IEmailService> _services;

        public EmailSendingController(ILogger<EmailSendingController> logger, IEnumerable<IEmailService> services)
        {
            _logger = logger;
            _services = services;
        }

        [HttpPost]
        public IActionResult SendEmail(EmailToSendApiModel model)
        {
            var service = _services.FirstOrDefault(s => s.Type == model.Type);

            if (service == null)
            {
                return BadRequest($"Type {model.Type} not supported.");
            }

            service.SendEmail(model.Email, model.Subject, model.Body);

            return Ok();
        }
    }
}