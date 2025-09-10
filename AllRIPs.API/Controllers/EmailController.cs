using Microsoft.AspNetCore.Mvc;
using AllRIPs.DTOS;
using AllRIPs.SERVICES;

namespace AllRIPs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EmailController : ControllerBase
    {
        private readonly EmailService _services;

        public EmailController(EmailService services)
        {
            _services = services;
        }

        [HttpPost]
        [Route("sendEmail")]
        public async Task<ActionResult<bool>> SendEmail(EmailDTO email)
        {
            return await _services.SendEmail(email);
        }

    }
}
