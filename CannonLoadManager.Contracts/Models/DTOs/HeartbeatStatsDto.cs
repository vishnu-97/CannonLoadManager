namespace CannonLoadManager.Contracts.Models.DTOs
{
    public class HeartbeatStatsDto
    {
        public int HeartbeatsRecievedWhenIdleCount { get; set; }
        public int HeartbeatsRecievedDuringTransactionCount { get; set; }
        public int TotalHeartbeatsRecieved { get; set; }
    }
}
