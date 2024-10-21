using CannonLoadManager.Contracts.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CannonLoadManager.Contracts.Models.Domains
{
    public class Device
    {
        public string InitBook { get; set; } = "No Init Book";
        public string MainBook { get; set; } = "No Main Book";
        public string ApiCall { get; set; } = "";
        public string LastResponse { get; set; } = "";
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        //public PinPadModel Model { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string TriPosIP { get; set; } = string.Empty;
        public string PinPadIP { get; set; } = string.Empty;
        public bool useTriposDirect { get; set; } = false;
        public bool GetDetailedLogs { get; set; } = false;

        [Range(1, 65535)]
        public int Port { get; set; } = 9001;

        [Range(1, 65535)]
        public int PinPadPort { get; set; } = 1;

        public Guid Guid { get; set; } = Guid.NewGuid();
    }
}
