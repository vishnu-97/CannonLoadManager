namespace CannonLoadManager.Contracts.Models.DTOs
{
    public class CannonManagerResponseDto
    { 
        public string Message { get; set; }
        public bool Success { get; set; }
        public string[] CannonAddresses { get; set; }

        public CannonManagerResponseDto()
        {
            Message= string.Empty;
            Success = false;
            CannonAddresses = Array.Empty<string>();
        }
    }
}
