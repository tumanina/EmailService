using EmailService.Interfaces;
using EmailService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers
{
    [ApiController]
    [Route("api")]
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
        [Route("text-emails")]
        public IActionResult SendTextEmail(TextEmailApiModel model)
        {
            var service = _services.FirstOrDefault(s => s.Provider == model.Provider);

            if (service == null)
            {
                return BadRequest($"Provider {model.Provider} not supported.");
            }

            service.SendEmail(model.Email, model.Subject, model.Body);

            return Ok();
        }

        [HttpPost]
        [Route("template-emails")]
        public IActionResult SendTemplateEmail(TemplateEmailApiModel model)
        {
            var service = _services.FirstOrDefault(s => s.Provider == model.Provider);

            if (service == null)
            {
                return BadRequest($"Provider {model.Provider} not supported.");
            }

            service.SendEmail(model.Email, model.TemplateId, model.TemplateParameters);

            return Ok();
        }
    }
}