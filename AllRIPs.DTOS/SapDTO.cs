
using SapNwRfc;

namespace AllRIPs.DTOS
{
    public class SapDTO
    {
        [SapName("I_DATOS")]
        public string? responseJson {  get; set; }
    }
}
