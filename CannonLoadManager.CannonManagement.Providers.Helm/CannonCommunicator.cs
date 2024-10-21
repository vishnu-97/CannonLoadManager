using CannonLoadManager.Contracts.Const;
using CannonLoadManager.Contracts.Interfaces;
using CannonLoadManager.Contracts.Models.Domains;
using CannonLoadManager.Contracts.Models.DTOs;
using CannonLoadManager.Contracts.Configurations;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Web;
using System.Data;

namespace CannonLoadManager.CannonManagement.Providers.Helm
{
    public class CannonCommunicator : ICannonCommunicator
    {
        private readonly ICannonManager _cannonManager;

        public CannonCommunicator(ICannonManager cannonManager)
        {
            _cannonManager = cannonManager;
        }

        public async Task<CannonManagerResponseDto> CreateCannonAsync(string apiRoute, HttpMethod apiMethod, string requestToken, int deviceCount, Device device, Dictionary<string, string>? headers = null)
        {
            var cannonManagerResponse = await _cannonManager.GetCannonAddressesAsync(requestToken).ConfigureAwait(false);

            if (!cannonManagerResponse.Success)
                return cannonManagerResponse;

            List<Task<CannonResponseDto>> allTasks = new();
            int serialNumber = int.TryParse(device.SerialNumber, out int tempNum) ? tempNum : 33335005;

            var createParams = new Dictionary<string, string>
            {
                {"deviceCount", deviceCount.ToString() }
            };
            foreach (string ipAddress in cannonManagerResponse.CannonAddresses)
            {               
                device.SerialNumber = serialNumber.ToString();
                allTasks.Add(SendApiAsync(ipAddress, apiRoute, apiMethod, createParams, device, headers));
                serialNumber += deviceCount;
            }

            var apiResponses = await Task.WhenAll(allTasks).ConfigureAwait(false);
            cannonManagerResponse.Message = "[" + string.Join(",\n", (apiResponses).Select(res => res.ToString())) + "]";
            cannonManagerResponse.Success = !apiResponses.Any(v => !v.Success);

            return cannonManagerResponse;
        }

        public async Task<CannonManagerResponseDto> CallCannonAsync(string apiRoute, HttpMethod apiMethod, string requestToken, Dictionary<string, string> parameters, object? body = null, Dictionary<string, string>? headers = null)
        {
            var cannonManagerResponse = await _cannonManager.GetCannonAddressesAsync(requestToken).ConfigureAwait(false);
            if (!cannonManagerResponse.Success)
                return cannonManagerResponse;

            var tasks = cannonManagerResponse.CannonAddresses.Select(o => SendApiAsync(o, apiRoute, apiMethod, parameters, body, headers));
            var results = await Task.WhenAll(tasks);
            cannonManagerResponse.Message = "[" + string.Join(",\n", results.Select(r => r.Message)) + "]";

            return cannonManagerResponse;
        }

        private async Task<CannonResponseDto> SendApiAsync(string ipAddress, string apiRoute, HttpMethod apiMethod, Dictionary<string, string> parameters, object? body = null, Dictionary<string, string>? headers = null)
        {
            var finalResponse = new CannonResponseDto(ipAddress);
            try
            {
                using var client = new HttpClient();

                parameters?.ToList().ForEach(parameter => apiRoute = apiRoute.Replace($"{{{parameter.Key}}}", parameter.Value));

                var uri = new Uri(new Uri($"http://{ipAddress.Replace('.', '-')}.default.pod.cluster.local:{ConfigurationSettings.CannonPort}", UriKind.Absolute), ApiRoutes.BaseCannonPath + apiRoute);
                //uri = new Uri(new Uri(ipAddress, UriKind.Absolute), ApiRoutes.BaseCannonPath + apiRoute);/// test tobe removed

                var request = new HttpRequestMessage
                {
                    Method = apiMethod,
                    RequestUri = uri
                };

                headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

                if (body != null)
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    finalResponse.Success = true;

                finalResponse.Message = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                finalResponse.Message = ex.Message;
            }
            return finalResponse;
        }
    }
}