using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using AllRIPs.DTOS;
using static AllRIPs.DTOS.RespuestaCargueFevRipsDTO;

namespace AllRIPs.SERVICES
{
    public class RipsService
    {
        private readonly IConfiguration Config;
        private readonly HttpClient _httpclient;

        public RipsService(IConfiguration config)
        {
            Config = config;
            var clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                MaxRequestContentBufferSize = 2147483647
            };

            _httpclient = new HttpClient(clientHandler)
            {
                Timeout = TimeSpan.FromMinutes(30)
            };
        }

        private async Task<string> AuthLoginSISPRO(AuthSISPRODTO Param)
        {
            string param = JsonConvert.SerializeObject(Param),
                fullUrl = $"{Config.GetValue<string>("EndPointDocker:Url")}api/Auth/LoginSISPRO";

            HttpClientHandler clientHandler = new();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpRequestMessage Request = new(HttpMethod.Post, fullUrl);
            HttpContent Content = new StringContent(param, Encoding.UTF8, "application/json");
            Request.Content = Content;

            HttpClient Client = new(clientHandler);
            HttpResponseMessage Response = await Client.SendAsync(Request);

            ResultLoginSISPRODTO? Result = await Response.Content.ReadFromJsonAsync<ResultLoginSISPRODTO>();
            if (Response.IsSuccessStatusCode)
            {
                if(Result!.token != null)
                {
                    return Result!.token;
                }else
                {
                    Serilog.Log.Error($"(SIOP-RIPS) Error autenticación sispro usuario {Param.persona} {Result!.errors[0]}");
                    throw new Exception(Result!.errors[0]);
                }
            }
            else
            {
                Serilog.Log.Error($"(SIOP-RIPS) Error autenticación sispro usuario {Param.persona} {Result!.errors[0]}");
                throw new Exception(Result!.errors[0]);
            }
        }

        public async Task<RespuestaCargueFevRipsDTO> CargarFevRipsJSON(CargueFevRipsDTO Json)
        {
            string
                endPoint = Json!.rips!.tipoNota == "NC" ? "CargarNC" : Json!.rips!.tipoNota == "NA" ? "CargarNotaAjuste" : "CargarFevRips",
                fullUrl = $"{Config.GetValue<string>("EndPointDocker:Url")}api/PaquetesFevRips/{endPoint}",
                base64Tipo = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Tipo")!,
                base64Numero = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Numero")!,
                base64Clave = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Clave")!,
                base64Nit = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Nit")!,
                Tipo = Encoding.UTF8.GetString(Convert.FromBase64String(base64Tipo)),
                Numero = Encoding.UTF8.GetString(Convert.FromBase64String(base64Numero)),
                Clave = Encoding.UTF8.GetString(Convert.FromBase64String(base64Clave)),
                Nit = Encoding.UTF8.GetString(Convert.FromBase64String(base64Nit));

            var token = await AuthLoginSISPRO(
                new AuthSISPRODTO
                {
                    persona =
                        {
                     identificacion =
                     {
                         tipo = Tipo,
                         numero = Numero,
                     }
                        },
                    clave = Clave,
                    nit = Nit
                }
            );

            var NewJson = new CargueFevRipsDTO
            {
                rips = Json.rips,
                xmlFevFile = Json.xmlFevFile
            };

            string param = JsonConvert.SerializeObject(NewJson, Formatting.Indented);

            // Configuración de la solicitud HTTP
            HttpRequestMessage Request = new(HttpMethod.Post, fullUrl);
            HttpContent Content = new StringContent(param, Encoding.UTF8, "application/json");
            Request.Content = Content;

            _httpclient.DefaultRequestHeaders.Clear();
            _httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");


            // Envío de la solicitud
            HttpResponseMessage Response = await _httpclient.SendAsync(Request);

            // Manejo de la respuesta
            if (Response.IsSuccessStatusCode)
            {
                var respuesta = await Response.Content.ReadFromJsonAsync<RespuestaCargueFevRipsDTO>();
                return respuesta!;
            }
            else
            {
                string json = await Response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var newResponse = new RespuestaCargueFevRipsDTO();

                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("ResultadosValidacion", out var resultadosProp) &&
                    resultadosProp.ValueKind == JsonValueKind.Array)
                {
                    newResponse = System.Text.Json.JsonSerializer.Deserialize<RespuestaCargueFevRipsDTO>(json);
                }
                else
                {
                    var errorDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    List<ResultadoValidacion> Errors = new ();

                    foreach (var entry in errorDict!)
                    {
                        foreach (var error in entry.Value)
                        {
                            var splitError = error.Split('|');
                            int countError = splitError.Length;
                            var codigo = countError == 1 ? "NOCODIGO" : splitError[0].Trim();
                            var descripcion = countError == 1 ? splitError[0].Trim() : splitError[1].Trim();
                            var observaciones = countError == 1 ? splitError[0].Trim() : splitError[1].Trim();
                            Errors.Add(new ResultadoValidacion
                            {
                                clase = "RECHAZADO",
                                codigo = codigo,
                                descripcion = descripcion,
                                observaciones = observaciones,
                                pathFuente = entry.Key,
                                fuente = "Rips"
                            });
                        }
                    }

                    newResponse = new RespuestaCargueFevRipsDTO
                    {
                        resultState = false,
                        procesoId = 0,
                        numFactura = Json.rips.numFactura!, // Aquí deberías extraer del JSON si está disponible
                        codigoUnicoValidacion = "",
                        codigoUnicoValidacionToShow = "",
                        fechaRadicacion = "", // Igual, extraer si es necesario
                        rutaArchivos = "",
                        resultadosValidacion = Errors
                    };
                }
                //Log.Error($"Error respuesta apidocker envio de rips {newResponse}");
                throw new Exception(JsonConvert.SerializeObject(newResponse));
            }
        }

        public async Task<RespuestaConsultarCUVDTO> ConsultarCUV(CargueCUVParam Json)
        {
            string fullUrl = $"{Config.GetValue<string>("EndPointDocker:Url")}api/ConsultasFevRips/ConsultarCUV",
                base64Tipo = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Tipo")!,
                base64Numero = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Numero")!,
                base64Clave = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Clave")!,
                base64Nit = Config.GetValue<string>($"EndPointDocker:{Json.einri}:Nit")!,
                Tipo = Encoding.UTF8.GetString(Convert.FromBase64String(base64Tipo)),
                Numero = Encoding.UTF8.GetString(Convert.FromBase64String(base64Numero)),
                Clave = Encoding.UTF8.GetString(Convert.FromBase64String(base64Clave)),
                Nit = Encoding.UTF8.GetString(Convert.FromBase64String(base64Nit));

            var token = await AuthLoginSISPRO(
                new AuthSISPRODTO
                {
                    persona =
                        {
                            identificacion =
                            {
                                tipo = Tipo,
                                numero = Numero,
                            }
                        },
                    clave = Clave,
                    nit = Nit
                }
            );

            var NewJson = new CargueCUVParam
            {
                codigoUnicoValidacion = Json.codigoUnicoValidacion
            };

            string param = JsonConvert.SerializeObject(NewJson, Formatting.Indented);

            HttpRequestMessage Request = new(HttpMethod.Post, fullUrl);
            HttpContent Content = new StringContent(param, Encoding.UTF8, "application/json");
            Request.Content = Content;
            _httpclient.DefaultRequestHeaders.Clear();
            _httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");


            HttpResponseMessage Response = await _httpclient.SendAsync(Request);

            var respuesta = await Response.Content.ReadAsStringAsync();

            if (Response.IsSuccessStatusCode)
            {
                RespuestaConsultarCUVDTO? Result = await Response.Content.ReadFromJsonAsync<RespuestaConsultarCUVDTO>();
                var newRespuestaConsultarCUVDTO = new RespuestaConsultarCUVDTO
                {
                    procesoId = Result!.procesoId,
                    esValido = Result.esValido,
                    codigoUnicoValidacion = Result.codigoUnicoValidacion,
                    fechaValidacion = Result.fechaValidacion,
                    numDocumentoIdObligado = Result.numDocumentoIdObligado,
                    numeroDocumento = Result.numeroDocumento,
                    fechaEmision = Result.fechaEmision,
                    totalFactura = Result.totalFactura,
                    cantidadUsuarios = Result.cantidadUsuarios,
                    cantidadAtenciones = Result.cantidadAtenciones,
                    totalValorServicios = Result.totalValorServicios,
                    identificacionAdquiriente = Result.identificacionAdquiriente,
                    codigoPrestador = Result.codigoPrestador,
                    modalidadPago = Result.modalidadPago,
                    numDocumentoReferenciado = Result.numDocumentoReferenciado,
                    urlJson = Result.urlJson,
                    urlXml = Result.urlXml,
                    jsonFile = Result.jsonFile,
                    xmlFileBase64 = Result.xmlFileBase64
                };
                return newRespuestaConsultarCUVDTO!;
            }
            else
            {
                throw new Exception(JsonConvert.SerializeObject(respuesta));
            }
        }

        public async Task<string> Version()
        {
            try
            {
                string fullUrl = $"{Config.GetValue<string>("EndPointDocker:Url")}api/TestApi/Index";

                HttpClientHandler clientHandler = new();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                HttpRequestMessage Request = new(HttpMethod.Get, fullUrl);

                HttpClient Client = new(clientHandler);
                HttpResponseMessage Response = await Client.SendAsync(Request);

                var Result = await Response.Content.ReadAsStringAsync();

                return Result;
            }
            catch (Exception ex)
            {
                return ($"sin respuesta error: {ex.Message} exception: {ex.InnerException?.Message}");
            }
        }
    }
}
