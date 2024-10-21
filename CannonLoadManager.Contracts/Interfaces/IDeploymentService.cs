using CannonLoadManager.Contracts.Models.DTOs;
using CliWrap;
using System.Net;

namespace CannonLoadManager.Contracts.Interfaces
{
    public interface IDeploymentService
    {
        public Task<bool> TryDeployPod(string requestToken, int podCount, out string message);
        public Task<bool> TryGetPodAddressesAsync(string requestToken, out string[] podAddresses);
        public Task<bool> TryDeployUninstall(string requestToken, out string message);
    }
}
