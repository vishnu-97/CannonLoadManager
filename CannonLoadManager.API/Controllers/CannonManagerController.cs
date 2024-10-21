using CannonLoadManager.Contracts.Configurations;
using CannonLoadManager.Contracts.Const;
using CannonLoadManager.Contracts.Interfaces;
using CannonLoadManager.Contracts.Models.Domains;
using CannonLoadManager.Contracts.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using CannonLoadManager.CannonManagement.Providers.Helm;

namespace CannonLoadManager.API.Controllers
{
    [ApiController]
    [Route("api/{Controller}")]
    public class CannonManagerController : ControllerBase
    {
        private readonly ILogger<CannonManagerController> _logger;
        private readonly ICannonManager _cannonManager;
        private readonly ICannonCommunicator _cannonCommunicator;

        private static Uri? Library { get; set; }
        private static HashSet<string> LoadTests { get; set; } = [];

        //Tobe removed in next phase
        private static string? CurrentRequestToken { get; set; }

        public CannonManagerController(ILogger<CannonManagerController> logger, ICannonManager cannonManager, ICannonCommunicator cannonCommunicator)
        {
            _logger = logger;
            _cannonManager = cannonManager;
            _cannonCommunicator = cannonCommunicator;
        }

        #region MDS Library Methods
        [HttpPut("LoadLibraryFromBlob")]
        public async Task<CannonLoadMangerResponseDto> LoadLibrary(Uri libraryFile)
        {
            var response = new CannonLoadMangerResponseDto
            {
                Success = true,
                Message = "Librarary file ready to be loaded"
            };
            try
            {
                if (libraryFile != null)
                {
                    using var httpClient = new HttpClient();
                    var httpRequest = new HttpRequestMessage { RequestUri = libraryFile, Method = HttpMethod.Get };
                    var fileRresponse = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
                    if (!fileRresponse.IsSuccessStatusCode)
                    {
                        throw new FileNotFoundException("Error while getting Library file: " + fileRresponse.ReasonPhrase);
                    }
                    Library = libraryFile;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Invalid Library uri";
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion

        #region Manage Devices
        [HttpPost("CreateDevice/{deviceCount}")]
        public async Task<CannonLoadMangerResponseDto> CreateNewDevice(Device deviceInfo, int deviceCount = 1)
        {
            var response = new CannonLoadMangerResponseDto();

            try
            {
                if (LoadTests.Count >= ConfigurationSettings.MaxAllowedLoadTests)
                {
                    response.Message = $"Cannot do more than {ConfigurationSettings.MaxAllowedLoadTests}. Please stop some of the existing load tests.";
                    return response;
                }
                if (Library == null)
                {
                    response.Message = $"No library loaded";
                    return response;
                }
                var totalCannons = (int)Math.Ceiling((decimal)deviceCount / ConfigurationSettings.MaxSimulatorsPerCannon);
                var devicesPerCannon = (int)Math.Ceiling((decimal)deviceCount / totalCannons);
                var requestToken = CreateRequestToken();

                CurrentRequestToken = requestToken;

                var cannonResponse = await _cannonManager.CreateAsync(requestToken, totalCannons).ConfigureAwait(false);

                if (!cannonResponse.Success)
                {
                    response.Message = cannonResponse.Message;
                    return response;
                }

                var libraryParams = new Dictionary<string, string>
                {
                    {"libraryFile", Library.ToString() }
                };
                var libraryCallResponse = await _cannonCommunicator.CallCannonAsync(ApiRoutes.LibraryBlobCall, HttpMethod.Put, requestToken, libraryParams).ConfigureAwait(false);
                if (!libraryCallResponse.Success)
                {
                    response.Message = libraryCallResponse.Message;
                    return response;
                }

                var createDevicesResponse = await _cannonCommunicator.CreateCannonAsync(ApiRoutes.CreateCall, HttpMethod.Post, requestToken, devicesPerCannon, deviceInfo).ConfigureAwait(false);

                response.Message = libraryCallResponse?.Message + "\n\n" + createDevicesResponse.Message;
                response.Success = createDevicesResponse.Success;
            }
            catch (Exception ex)
            {
                //Add Logs for detailed response
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }


        [HttpPut("StartAllDevices/{timerResponse}")]
        public async Task<CannonLoadMangerResponseDto> StartAllDevices(string timerResponse = "")
        {
            var response = new CannonLoadMangerResponseDto();

            try
            {
                if (CurrentRequestToken == null)
                {
                    response.Message = $"No Load tests running";
                    return response;
                }
                var startParams = new Dictionary<string, string>
                {
                    {nameof(timerResponse), timerResponse }
                };

                var startResponse = await _cannonCommunicator.CallCannonAsync(ApiRoutes.StartCall, HttpMethod.Put, CurrentRequestToken, startParams).ConfigureAwait(false);

                response = CannonLoadMangerResponseDto.CreateResponse(startResponse);
            }
            catch(Exception ex)
            {
                //Add Logs for detailed response
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        //[HttpPut("StartAllDevices/{maxDevices}/{timerResponse}")]
        //public object StartDevicesLimited(int maxDevices, string timerResponse = "") => _PerfomanceDevices.StartDevicesLimited(maxDevices, timerResponse);

        //[HttpPut("StartDevices/{deviceCount}/{timerResponse}")]
        //public object StartDevices(int deviceCount, string timerResponse = "") => _PerfomanceDevices.StartDevices(deviceCount, timerResponse);

        [HttpPatch("RestartAllDevices/{timerResponse}")]
        public async Task<CannonLoadMangerResponseDto> RestartAllDevices(string timerResponse)
        {
            var response = new CannonLoadMangerResponseDto();

            try
            {
                if (CurrentRequestToken == null)
                {
                    response.Message = $"No Load tests running";
                    return response;
                }
                var startParams = new Dictionary<string, string>
                {
                    {nameof(timerResponse), timerResponse }
                };

                var startResponse = await _cannonCommunicator.CallCannonAsync(ApiRoutes.RestartCall, HttpMethod.Patch, CurrentRequestToken, startParams).ConfigureAwait(false);

                response = CannonLoadMangerResponseDto.CreateResponse(startResponse);
            }
            catch (Exception ex)
            {
                //Add Logs for detailed response
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        //[HttpPatch("RestartAllDevices/{maxDevices}/{timerResponse}")]
        //public async Task<object> RestartAllDevices(int maxDevices, string timerResponse) => await _PerfomanceDevices.RestartAllDevices(maxDevices, timerResponse);

        [HttpPut("StopAllDevices")]
        public async Task<IEnumerable<CannonLoadMangerResponseDto>> StopAllDevices() 
        {
            var response = new List<CannonLoadMangerResponseDto>();

            try
            {
                if (CurrentRequestToken == null)
                {
                    var errorResp = new CannonLoadMangerResponseDto() { Message = $"No Load tests running" };
                    response.Add(errorResp);
                    return  response;
                }

                var stopResponse = await _cannonCommunicator.CallCannonAsync(ApiRoutes.StopCall, HttpMethod.Put, CurrentRequestToken, null).ConfigureAwait(false);

                response.Add(CannonLoadMangerResponseDto.CreateResponse(stopResponse));
            }
            catch (Exception ex)
            {
                //Add Logs for detailed response
                var errorResp = new CannonLoadMangerResponseDto() { Message = ex.Message, Level = LogLevel.Error };
                response.Add(errorResp);
            }
            return response;
        }

        [HttpDelete("RemoveAllDevices")]
        public async Task<IEnumerable<CannonLoadMangerResponseDto>> RemoveAllDevices()
        {
            var response = new List<CannonLoadMangerResponseDto>();

            try
            {
                if (CurrentRequestToken == null)
                {
                    var errorResp = new CannonLoadMangerResponseDto() { Message = $"No Load tests running" };
                    response.Add(errorResp);
                    return response;
                }

                var stopResponse = await _cannonCommunicator.CallCannonAsync(ApiRoutes.RemoveCall, HttpMethod.Delete, CurrentRequestToken, null).ConfigureAwait(false);

                var cannonResponse = await _cannonManager.RemoveCannonServiceAsync(CurrentRequestToken).ConfigureAwait(false);

                if (!cannonResponse.Success)
                {
                    var errorResp = new CannonLoadMangerResponseDto() { Message = cannonResponse.Message };
                    response.Add(errorResp);
                    return response;
                }
                else
                {
                    LoadTests.Remove(CurrentRequestToken);
                    CurrentRequestToken = null;
                }

                response.Add(CannonLoadMangerResponseDto.CreateResponse(stopResponse));
            }
            catch (Exception ex)
            {
                //Add Logs for detailed response 85360341
                var errorResp = new CannonLoadMangerResponseDto() { Message = ex.Message, Level = LogLevel.Error };
                response.Add(errorResp);
            }
            return response;
        }
        #endregion

        //#region Get Device Reports

        //[HttpGet("GetDeviceStats")]
        //public IEnumerable<MdsDeviceStatsDto> GetDeviceStats()
        //{

        //}

        //[HttpGet("GetDeviceStats/{deviceResult}")]
        //public IEnumerable<MdsDeviceStatsDto> GetDeviceStats(DeviceResult deviceResult) => _PerfomanceDevices.GetDeviceStats(deviceResult);

        //[HttpGet("GetDeviceResults")]
        //public DeviceStateRecord GetDeviceStates() => _PerfomanceDevices.GetDeviceStates();

        //[HttpGet("GetFullReport")]
        //public object GetFullReport() => _PerfomanceDevices.GetFullReport();

        //[HttpGet("CheckDeviceTimers/{deviceResult}")]
        //public object CheckDeviceTimers(DeviceResult deviceResult) => _PerfomanceDevices.CheckDeviceTimers(deviceResult);

        //#endregion

        //#region User Delays

        //[HttpPut("ToggleDelay/{toggleValue}")]
        //public MdsHarnessResponseDto ToggleDelay(bool toggleValue) => _PerfomanceDevices.ToggleDelay(toggleValue);

        //[HttpPut("LoadDelays")]
        //public MdsHarnessResponseDto LoadDelays(UserDelaySet_Model userDelay) => _PerfomanceDevices.LoadDelays(userDelay);

        //[HttpPut("SetDelays/{minValue}/{maxValue}")]
        //public MdsHarnessResponseDto SetDelays(int minValue, int maxValue) => _PerfomanceDevices.SetDelays(minValue, maxValue);

        //#endregion

        //#region CommandDelays
        //[HttpPut("ToggleCommandDelay/{toggleValue}")]
        //public MdsHarnessResponseDto ToggleCommandDelay(bool toggleValue) => _PerfomanceDevices.ToggleCommandDelay(toggleValue);

        //#endregion

        //[HttpPut("SetConnectionTimeout/{timeoutMS}")]
        //public MdsHarnessResponseDto SetConnectionTimeout(int timeoutMS) => _PerfomanceDevices.SetConnectionTimeout(timeoutMS);

        //[HttpPut("SetReconnect/{reconnectString}")]
        //public MdsHarnessResponseDto SetReconnectString(string reconnectString) => _PerfomanceDevices.SetReconnectString(reconnectString);

        private string CreateRequestToken()
        {
            while (true)
            {
                var requestToken = Guid.NewGuid().ToString();
                if (!LoadTests.Contains(requestToken))
                {
                    LoadTests.Add(requestToken);
                    return requestToken;
                }
            }
        }

    }
}