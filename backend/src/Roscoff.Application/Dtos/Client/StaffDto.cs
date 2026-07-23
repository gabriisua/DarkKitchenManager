using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Dtos.Client;

public class StaffQueryParameters : BasePaginationRequestDto
{
    public string? Email { get; set; }
    public string? Role { get; set; }
}

public record StaffCreateDto(string Username, string Email, string Password, string Role);

public record StaffReadDto(Guid Id, string Username, string Email, string Role, bool IsActive, DateTime? LastLogin);

public record StaffUpdateDto(string Email, string? Password, string Role);