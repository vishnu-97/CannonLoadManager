using Microsoft.Extensions.Logging;

namespace CannonLoadManager.Contracts.Models.DTOs
{
    public class CannonLoadMangerResponseDto
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public LogLevel Level { get; set; }

        public CannonLoadMangerResponseDto()
        {
            Message = string.Empty;
            Success = false;
            Level = LogLevel.Error;
        }

        public static CannonLoadMangerResponseDto CreateResponse(CannonManagerResponseDto response)
        {
            return new CannonLoadMangerResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Level = response.Success ? LogLevel.Information : LogLevel.Error
            };
        }
    }
}
