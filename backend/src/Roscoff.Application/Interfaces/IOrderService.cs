using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Core.Enums;

namespace Roscoff.Application.Interfaces;

public interface IOrderService
{
    Task<(bool Success, string Message, OrderResponseDto? Data)> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default);
    
    Task<PaginatedResponseDto<OrderResponseDto>> GetPagedAsync(OrderQueryParameters filter, CancellationToken cancellationToken = default);
    
    Task<OrderResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<(bool Success, string Message)> UpdateStatusAsync(Guid id, OrderStatus newStatus, CancellationToken cancellationToken = default);
}