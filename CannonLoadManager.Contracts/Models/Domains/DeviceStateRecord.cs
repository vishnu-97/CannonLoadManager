using CannonLoadManager.Contracts.Enums;
using CannonLoadManager.Contracts.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CannonLoadManager.Contracts.Models.Domains
{
    public class DeviceStateRecord
    {
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public DeviceStateDto[] DeviceStates { get; set; }

        public bool IsSuccessful => DeviceStates.All(o => o.DeviceState == DeviceResult.Success);

        public static DeviceStateRecord Combine(IEnumerable<DeviceStateRecord> records)
        {
            var results = records.SelectMany(o => o.DeviceStates);
            var groupedResults = results.GroupBy(o => o.DeviceState);
            var combinedResults = groupedResults.Select(o => new DeviceStateDto() { DeviceState = o.Key, DeviceCount = o.Sum(p => p.DeviceCount) }).ToArray();
            var dsr = new DeviceStateRecord()
            {
                TimeStamp = records.Min(o => o.TimeStamp),
                DeviceStates = combinedResults
            };
            return dsr;
        }
    }

}
