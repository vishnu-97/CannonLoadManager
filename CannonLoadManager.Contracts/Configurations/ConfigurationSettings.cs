using System.ComponentModel.DataAnnotations;

namespace CannonLoadManager.Contracts.Configurations
{
    public class ConfigurationSettings
    {
        [Required]
        public static int MaxAllowedLoadTests { get; set; }
        [Required]
        public static string CannonPort { get; set; }
        [Required]
        public static int MaxPodsperService { get; set; }
        [Required]
        public static string ServicePort { get; set; }
        [Required]
        public static string ChartName { get; set; }
        [Required]
        public static int MaxSimulatorsPerCannon { get; set; }
    }
}
