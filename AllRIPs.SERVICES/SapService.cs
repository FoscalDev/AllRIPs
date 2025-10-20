using System.Text;
using System.Text.Json;
using AllRIPs.DTOS;
using Microsoft.Extensions.Configuration;
using SapNwRfc;

namespace AllRIPs.SERVICES
{
    public class SapService
    {
        private readonly IConfiguration _config;
        private readonly string KeyPathSAP = "SAPConfiguration";

        public SapService(IConfiguration config)
        {
            _config = config;
        }

        public void UpdateStateInvoice(ResponseUploadFevRipsDTO Json)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(Json);
                string Server = _config.GetValue<string>($"{KeyPathSAP}:Server")!;
                string Id = _config.GetValue<string>($"{KeyPathSAP}:Id")!;
                string Mandt = _config.GetValue<string>($"{KeyPathSAP}:Mandt")!;
                string User = _config.GetValue<string>($"{KeyPathSAP}:User")!;
                string Trace = _config.GetValue<string>($"{KeyPathSAP}:Trace")!;
                string Password = Encoding.UTF8.GetString(Convert.FromBase64String(_config.GetValue<string>($"{KeyPathSAP}:Password")!));

                string connectionSap = $"AppServerHost={Server}; " +
                    $"SystemNumber={Id}; " +
                    $"User={User}; " +
                    $"Password={Password}; " +
                    $"Client={Mandt}; " +
                    $"Lenguage=EN; " +
                    $"PoolSize=5; " +
                    $"Trace={Trace};";

                using var connection = new SapConnection(connectionSap);
                connection.Connect();

                using var FunctionRfc = connection.CreateFunction("ZMF_RIPS_GUARDAR_ESTADO_ALLRIP");
                var responseRfc = FunctionRfc.Invoke<SapDTO>(new SapDTO
                {
                    responseJson = jsonString,
                });
                Console.WriteLine("✅ RFC ejecutado con JSON enviado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al ejecutar RFC: {ex.Message}");
                throw;
            }
        }
    }
}
