using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Interfaces;

public interface ICustomerService
{
    Task<PaginatedResponseDto<CustomerReadDto>> GetAllAsync(CustomerQueryParameters parameters);
    Task<CustomerDetailsDto?> GetByIdAsync(Guid id);
    Task<(bool Success, string Message, Guid? CustomerId)> CreateAsync(CustomerCreateDto request);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, CustomerUpdateDto request);
    Task<(bool Success, string Message)> DeleteAsync(Guid id); // Soft Delete
}