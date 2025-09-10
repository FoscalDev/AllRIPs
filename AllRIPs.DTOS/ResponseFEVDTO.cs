namespace AllRIPs.DTOS
{
    public class ResponseFEVDTO
    {
        public string software { get; set; }
        public bool success { get; set; }
        public string? msg { get; set; }
        public string? metodo { get; set; }
        public string? time { get; set; }
        public Data? data { get; set; }
        public MetaData? metaData { get; set; }
    }

    public class Data
    {
        public string? salida { get; set; }
        public string? attachedDocument { get; set; }
    }

    public class MetaData
    {
        // Puedes añadir propiedades adicionales de metaData si es necesario
    }

}
