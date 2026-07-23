using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Interfaces;

public interface IMenuService
{
    Task<PaginatedResponseDto<MenuResponseDto>> GetAllAsync(MenuQueryParameters filter, CancellationToken cancellationToken = default);
    Task<MenuResponseDto?> GetByIdAsync(int id);
    Task<MenuResponseDto> CreateAsync(MenuCreateDto dto);
    Task<MenuResponseDto?> UpdateAsync(int id, MenuUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
