using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Interfaces;

public interface IClientDiscountService
{
    Task<int> GetEffectivePriceAsync(Guid customerId, int plateId, CancellationToken cancellationToken = default);
    
    Task<PaginatedResponseDto<PlateDiscountDto>> GetPagedPlateDiscountsAsync(DiscountQueryParameters parameters);
    Task<PaginatedResponseDto<CategoryDiscountDto>> GetPagedCategoryDiscountsAsync(DiscountQueryParameters parameters);
    
    Task<(bool Success, string Message)> DeletePlateDiscountAsync(Guid customerId, int plateId);
    Task<(bool Success, string Message)> DeleteCategoryDiscountAsync(Guid customerId, int categoryId);
}