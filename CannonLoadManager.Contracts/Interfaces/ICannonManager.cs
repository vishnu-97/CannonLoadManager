using CannonLoadManager.Contracts.Models.DTOs;

namespace CannonLoadManager.Contracts.Interfaces
{
    public interface ICannonManager
    {
        public Task<CannonManagerResponseDto> CreateAsync(string requestToken, int cannonCount);
        public Task<CannonManagerResponseDto> GetCannonAddressesAsync(string requestToken);
        public Task<CannonManagerResponseDto> RemoveCannonServiceAsync(string requestToken);
    }
}
