namespace CannonLoadManager.Contracts.Models.Domains
{
    public class CannonService
    {
        public string Id { get; set; }
        public string CannonCount { get; set; }
        public List<Uri> CannonAddresses { get; set; }
    }
}
