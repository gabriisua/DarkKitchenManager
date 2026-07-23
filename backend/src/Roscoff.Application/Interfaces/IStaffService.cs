using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Interfaces;

public interface IStaffService
{
    Task<PaginatedResponseDto<StaffReadDto>> GetAllAsync(StaffQueryParameters parameters, CancellationToken cancellationToken = default);
    
    Task<StaffReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<StaffReadDto> CreateAsync(StaffCreateDto dto, CancellationToken cancellationToken = default);
    
    Task<bool> UpdateAsync(Guid id, StaffUpdateDto dto, CancellationToken cancellationToken = default);
    
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default); 
}