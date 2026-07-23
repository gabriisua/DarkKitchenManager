using Roscoff.Application.Dtos.Client;

namespace Roscoff.Application.Interfaces;

public interface IDeliveryHubService
{
    Task<IEnumerable<DeliveryHubDto>> GetByCustomerIdAsync(Guid customerId);
    Task<(bool Success, string Message, Guid? HubId)> CreateAsync(Guid customerId, DeliveryHubCreateDto request);
    Task<(bool Success, string Message)> UpdateAsync(Guid customerId, Guid hubId, DeliveryHubUpdateDto request);
    Task<(bool Success, string Message)> DeleteAsync(Guid customerId, Guid hubId);
}