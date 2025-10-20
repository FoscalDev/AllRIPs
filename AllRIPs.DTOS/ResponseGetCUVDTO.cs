using System.ComponentModel.DataAnnotations.Schema;

namespace AllRIPs.DTOS
{
    public class ResponseGetCUVDTO
    {
        public int procesoId { get; set; }
        public bool esValido { get; set; }
        public string? codigoUnicoValidacion { get; set; }
        public string? fechaValidacion { get; set; }
        public string? numDocumentoIdObligado { get; set; }
        public string? numeroDocumento { get; set; }
        public string? fechaEmision { get; set; }
        public decimal totalFactura { get; set; }
        public int cantidadUsuarios { get; set; }
        public int cantidadAtenciones { get; set; }
        public decimal totalValorServicios { get; set; }
        public string? identificacionAdquiriente { get; set; }
        public string? codigoPrestador { get; set; }
        public string? modalidadPago { get; set; }
        public string? numDocumentoReferenciado { get; set; }
        public string? urlJson { get; set; }
        public string? urlXml { get; set; }
        public string? jsonFile { get; set; }
        public string? xmlFileBase64 { get; set; }
        [NotMapped]
        public List<ResultadoValidacionDTO>? resultadosValidacion { get; set; }
    }

    public class ResultadoValidacionDTO
    {
        public string? clase { get; set; }
        public string? codigo { get; set; }
        public string? descripcion { get; set; }
        public string? observaciones { get; set; }
        public string? pathFuente { get; set; }
        public string? fuente { get; set; }
    }

}
