using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Interfaces;

public interface IAllergenService
{
    Task<PaginatedResponseDto<AllergenResponseDto>> GetAllAsync(AllergenQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IEnumerable<AllergenResponseDto>> GetAllAllergensAsync(CancellationToken cancellationToken = default);
    Task<AllergenResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? AllergenId)> CreateAsync(AllergenCreateDto request);
    Task<(bool Success, string Message)> UpdateAsync(int id, AllergenUpdateDto request);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}