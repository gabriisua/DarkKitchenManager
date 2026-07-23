using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Interfaces;

public interface IIngredientService
{
    Task<PaginatedResponseDto<IngredientResponseDto>> GetAllAsync(IngredientQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IngredientResponseDto> CreateIngredientAsync(CreateIngredientDto dto);
    Task<IngredientResponseDto?> GetIngredientByIdAsync(int id);
    Task<IngredientResponseDto?> UpdateIngredientAsync(int id, UpdateIngredientDto dto);
    Task<bool> SoftDeleteIngredientAsync(int id);
}