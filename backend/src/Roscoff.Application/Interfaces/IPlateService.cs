using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Interfaces;

public interface IPlateService
{
    Task<PaginatedResponseDto<PlateResponseDto>> GetAllAsync(PlateQueryParameters filter, CancellationToken cancellationToken = default);
    Task<PlateResponseDto> CreatePlateAsync(RequestPlateDto dto);
    Task<PlateResponseDto?> GetPlateWithIngredientsAsync(int id);
    Task<PlateResponseDto?> UpdatePlateAsync(int id, UpdatePlateDto dto);
    Task<bool> SoftDeletePlateAsync(int id);
    Task<byte[]?> GenerateTechnicalSheetPdfAsync(int id);
}