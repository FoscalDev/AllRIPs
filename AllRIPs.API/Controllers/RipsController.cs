using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AllRIPs.DTOS;
using AllRIPs.SERVICES;

namespace AllRIPs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RipsController : ControllerBase
    {
        private readonly RipsService _Services;

        public RipsController(RipsService Services)
        {
            _Services = Services;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CargarFevRips")]
        public async Task<ActionResult<RespuestaCargueFevRipsDTO>> CargarFevRips(CargueFevRipsDTO Param)
        {
            try
            {
                var paramCargueRips = new CargueFevRipsDTO
                {
                    rips = Param.rips,
                    xmlFevFile = Param.xmlFevFile,
                    einri = Param.einri
                };
                return await _Services.CargarFevRipsJSON(paramCargueRips);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"(SIOP - RIPs) {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ConsultarCUV")]
        public async Task<ActionResult<RespuestaConsultarCUVDTO>> ConsultarCUV(CargueCUVParam Param)
        {
            try
            {
                var paramCargueRips = new CargueCUVParam
                {
                    codigoUnicoValidacion = Param.codigoUnicoValidacion,
                    einri = Param.einri
                };
                return await _Services.ConsultarCUV(paramCargueRips);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"(SIOP - RIPs) {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Version")]
        public async Task<ActionResult<string>> Version()
        {
            return await _Services.Version();
        }

        [HttpPost("format")]
        public IActionResult FormatJson([FromBody] string jsonInput)
        {
            try
            {
                // Parsear el JSON para asegurarse de que es válido
                var parsedJson = JsonConvert.DeserializeObject(jsonInput);

                // Formatear el JSON con indentación
                var formattedJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

                // Devolver el JSON formateado
                return Ok(formattedJson);
            }
            catch (JsonException ex)
            {
                // Manejar errores de JSON inválido
                return BadRequest($"JSON inválido: {ex.Message}");
            }
        }
    }
}
