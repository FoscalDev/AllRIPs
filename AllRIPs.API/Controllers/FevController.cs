using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AllRIPs.FEV;

namespace AllRIPs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FevController : ControllerBase
    {
        private readonly ServicesFEV _service;
        private readonly IConfiguration Config;

        public FevController(ServicesFEV service, IConfiguration config)
        {
            _service = service;
            Config = config;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("getXML/{id}/{einri}")]
        public async Task<ActionResult<string>> GetXML(int id, int einri)
        {
            string
                base64User = $"{Config.GetValue<string>($"MyEnvoice:{einri}:Usuario")}",
                base64Password = $"{Config.GetValue<string>($"MyEnvoice:{einri}:Password")}",
                User = Encoding.UTF8.GetString(Convert.FromBase64String(base64User)),
                Password = Encoding.UTF8.GetString(Convert.FromBase64String(base64Password));

            return await _service.GetXml(id, einri, User, Password);
        }
    }
}
