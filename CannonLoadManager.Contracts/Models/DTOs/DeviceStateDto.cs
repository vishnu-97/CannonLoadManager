using CannonLoadManager.Contracts.Enums;

namespace CannonLoadManager.Contracts.Models.DTOs
{ 
    public class DeviceStateDto
    {
        public int DeviceCount { get; set; }
        public DeviceResult DeviceState { get; set; }
    }
}
