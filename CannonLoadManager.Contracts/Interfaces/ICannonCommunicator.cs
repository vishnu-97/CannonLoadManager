using CannonLoadManager.Contracts.Models.Domains;
using CannonLoadManager.Contracts.Models.DTOs;

namespace CannonLoadManager.Contracts.Interfaces
{
    public interface ICannonCommunicator
    {
        public Task<CannonManagerResponseDto> CreateCannonAsync(string apiRoute, HttpMethod apiMethod, string requestToken, int deviceCount, Device device, Dictionary<string, string>? headers = null);
        public Task<CannonManagerResponseDto> CallCannonAsync(string apiRoute, HttpMethod apiMethod, string requestToken, Dictionary<string, string> parameters, object? body = null, Dictionary<string, string>? headers = null);
    }
}
