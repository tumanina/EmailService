using Microsoft.AspNetCore.Mvc;

namespace EmailService1.Controllers
{
    [Route("api/[controller]")]
    public class PingController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "response";
        }
    }
}
