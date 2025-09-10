namespace AllRIPs.DTOS
{
    public class EmailDTO
    {
        public List<string>? Destinatarios { get; set; }
        public List<string>? CCDestinatarios { get; set; }
        public string? Adjunto { get; set; }
        public string? TipoEmail { get; set; }

        public string? Asunto { get; set; }
        public string? cuerpo { get; set; }

        public string? nombreadjunto { get; set; }

        public string? TipoCorreo { get; set; }

        public string? ClaveArchivo { get; set; }

    }
}
