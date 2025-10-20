using System.Data;
using AllRIPs.DTOS;
using AllRIPs.INTERFACES;
using AllRIPs.SERVICES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AllRIPs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RipsController : ControllerBase
    {
        private readonly RipsService _Services;
        private readonly SapService _ServiceSap;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<RipsController> _logger;

        public RipsController(RipsService Services, SapService ServiceSap, IBackgroundTaskQueue taskQueue, ILogger<RipsController> logger)
        {
            _Services = Services;
            _ServiceSap = ServiceSap;
            _taskQueue = taskQueue;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CargarFevRips")]
        [HttpPost]
        public Task<ActionResult<ResponseUploadFevRipsDTO>> CargarFevRips([FromBody] UploadFevRipsDTO Param)
        {
            try
            {
                _taskQueue.QueueBackgroundWorkItem(async _ =>
                {
                    try
                    {
                        var paramCargueRips = new UploadFevRipsDTO
                        {
                            rips = Param.rips,
                            xmlFevFile = Param.xmlFevFile,
                            einri = Param.einri
                        };

                        await Task.Delay(1 * 60 * 1000);
                        //var responseMinistry = await _Services.CargarFevRipsJSON(paramCargueRips);
                        var responseMinistry = new ResponseUploadFevRipsDTO
                        {
                            resultState = false,
                            procesoId = 0,
                            numFactura = Param.rips?.numFactura,
                            codigoUnicoValidacion = "No aplica a paquetes procesados en estado [RECHAZADO] o validaciones realizadas antes del envío al Ministerio de Salud y Protección Social",
                            fechaRadicacion = "2025-10-06T16:10:42.8067959-05:00",
                            rutaArchivos = null,
                            resultadosValidacion = new List<ResponseUploadFevRipsDTO.ResultadoValidacion>
                            {
                                new ResponseUploadFevRipsDTO.ResultadoValidacion
                                {
                                    clase = "RECHAZADO",
                                    codigo = "RVC012",
                                    descripcion = "El código del prestador de servicios de salud o del obligado a reportar debe estar relacionado con el numDocumentoIdObligado.",
                                    observaciones = "Verificar tablas de referencia, datos (Código Prestador:682760166601 - Nit: 900330752)",
                                    pathFuente = "usuarios[0].servicios.consultas[0].codPrestador",
                                    fuente = "Rips"
                                },
                                new ResponseUploadFevRipsDTO.ResultadoValidacion
                                {
                                    clase = "NOTIFICACION",
                                    codigo = "RVC017",
                                    descripcion = "El código de CUPS puede ser validado que corresponda a la cobertura o plan de beneficios informada en la factura electrónica de venta.",
                                    observaciones = "Verificar tabla de referencia Dato (890702)",
                                    pathFuente = "usuarios[0].servicios.consultas[0].codConsulta",
                                    fuente = "Rips"
                                }
                            }
                        };
                        _ServiceSap.UpdateStateInvoice(responseMinistry);

                        _logger.LogInformation("Procesamiento completado para factura {Factura}", Param.rips?.numFactura);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando factura {Factura}", Param.rips?.numFactura);
                    }

                    // Esto garantiza que el delegado asincrónico complete correctamente
                    await Task.CompletedTask;
                });

                var fastResponse = new ResponseUploadFevRipsDTO
                {
                    resultState = false,
                    procesoId = 0,
                    numFactura = Param.rips?.numFactura!,
                    codigoUnicoValidacion = "",
                    codigoUnicoValidacionToShow = "",
                    fechaRadicacion = "",
                    rutaArchivos = "",
                    resultadosValidacion = new List<ResponseUploadFevRipsDTO.ResultadoValidacion>
            {
                new()
                {
                    clase = "PROCESANDO...",
                    codigo = "",
                    descripcion = "Archivo de RIPs recibido y procesándose en segundo plano.",
                    observaciones = "Archivo de RIPs recibido y procesándose en segundo plano.",
                    pathFuente = "",
                    fuente = ""
                }
            }
                };

                return Task.FromResult<ActionResult<ResponseUploadFevRipsDTO>>(Ok(fastResponse));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"(SIOP - RIPs) {ex.Message}");

                var responseFailure = new ResponseUploadFevRipsDTO
                {
                    resultState = false,
                    codigoUnicoValidacion = "",
                    procesoId = 0,
                    numFactura = "",
                    codigoUnicoValidacionToShow = "",
                    fechaRadicacion = "",
                    rutaArchivos = "",
                    resultadosValidacion = new List<ResponseUploadFevRipsDTO.ResultadoValidacion>
            {
                new()
                {
                    clase = "ERROR",
                    codigo = "EXCEPTION",
                    descripcion = ex.Message,
                    observaciones = ex.StackTrace ?? "",
                    pathFuente = "",
                    fuente = "CargarFevRips"
                }
            }
                };

                return Task.FromResult<ActionResult<ResponseUploadFevRipsDTO>>(BadRequest(responseFailure));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ConsultarCUV")]
        public async Task<ActionResult<ResponseGetCUVDTO>> ConsultarCUV([FromBody] CargueCUVParam Param)
        {
            try
            {
                var paramCargueRips = new CargueCUVParam
                {
                    codigoUnicoValidacion = Param.codigoUnicoValidacion,
                    einri = Param.einri
                };

                var responseGetCUV = await _Services.ConsultarCUV(paramCargueRips);
                return Ok(responseGetCUV);
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
    }
}
