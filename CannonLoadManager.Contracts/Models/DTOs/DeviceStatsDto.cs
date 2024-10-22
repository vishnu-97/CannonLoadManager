using CannonLoadManager.Contracts.Enums;

namespace CannonLoadManager.Contracts.Models.DTOs
{
    public class DeviceStatsDto
    {
        public Guid RequestId { get; set; }
        public Guid? ConnectId { get; set; }
        public PinPadModel Model { get; set; }
        public DateTime FinishedTimeStamp { get; set; } = DateTime.Now;
        public int ConnectTime { get; set; }
        public int RestartCount { get; set; }
        public DeviceResult Result { get; set; }
        public string SerialNumber { get; set; }
        public HeartbeatStatsDto HeartbeatStats { get; set; }
    }
}
