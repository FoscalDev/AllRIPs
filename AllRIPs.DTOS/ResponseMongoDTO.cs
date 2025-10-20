namespace AllRIPs.DTOS
{
    public class ResponseMongoDTO
    {  
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? numFactura { get; set; }
        public ResponseUploadFevRipsDTO? data { get; set; }
    }
}
