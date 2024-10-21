namespace CannonLoadManager.Contracts.Const
{
    public static class ApiRoutes
    {
        public const string BaseCannonPath = "/api/MdsCannon/";
        public const string LibraryBlobCall = "LoadLibraryFromBlob?libraryFile={libraryFile}";
        public const string CreateCall = "CreateDevice/{deviceCount}";
        //public const string CreateAndStart = "CreateAndStart/{deviceCount}/{timerResponse}";
        public const string StartCall = "StartAllDevices/{timerResponse}";
        //public const string StartMaxDeviceCall = "StartAllDevices/{maxDevices}/{timerResponse}";
        //public const string StartDevicesCall = "StartDevices/{deviceCount}/{timerResponse}";
        public const string RestartCall = "RestartAllDevices/{timerResponse}";
        //public const string RestartMaxDeviceCall = "RestartAllDevices/{maxDevices}/{timerResponse}";
        public const string StopCall = "StopAllDevices";
        public const string RemoveCall = "RemoveAllDevices";
        public const string DeviceStatsCall = "GetDeviceStats";
        public const string DeviceResultStatsCall = "GetDeviceStats/{deviceResult}";
        public const string GetDeviceResultsCall = "GetDeviceResults";
        public const string ReportCall = "GetFullReport";
        public const string StatusCall = "CheckDeviceTimers/{deviceResult}";
        public const string ToggleDelayCall = "ToggleDelay/{toggleValue}";
        public const string LoadDelaysCall = "LoadDelays";
        public const string SetDelaysCall = "SetDelays/{minValue}/{maxValue}";
        public const string ToggleCommandDelayCall = "ToggleCommandDelay/{toggleValue}";
        public const string TimeoutCall = "SetConnectionTimeout/{timeoutMS}";
        public const string ReconnectCall = "SetReconnect/{reconnectString}";
    }
}
