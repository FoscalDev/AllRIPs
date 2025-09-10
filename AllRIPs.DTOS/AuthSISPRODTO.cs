namespace AllRIPs.DTOS
{
    public class AuthSISPRODTO
    {
        public PersonaDTO persona { get; set; } = new PersonaDTO();
        public string clave { get; set; }
        public string nit { get; set; }
    }

    public class PersonaDTO
    {
        public identificacionDTO identificacion { get; set; } = new identificacionDTO();
    }

    public class identificacionDTO
    {
        public string tipo { get; set; }
        public string numero { get; set; }
    }

    public class ResultLoginSISPRODTO
    {
        public string token { get; set; }
        public bool login { get; set; }
        public bool registrado { get; set; }
        public List<string> errors { get; set; }
    }

}
