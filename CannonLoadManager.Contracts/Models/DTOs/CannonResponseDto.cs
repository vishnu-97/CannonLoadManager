using Newtonsoft.Json;

namespace CannonLoadManager.Contracts.Models.DTOs
{
    public class CannonResponseDto 
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string IpAddress { get; set; }

        public CannonResponseDto(string ipAddress)
        {
            Message = string.Empty;
            Success = false;
            IpAddress = ipAddress;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
