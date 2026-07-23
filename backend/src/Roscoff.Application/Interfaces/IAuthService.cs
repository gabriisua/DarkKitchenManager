using Roscoff.Application.Dtos.Auth;

namespace Roscoff.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<(string Messaggio, string? Token)> ResetPasswordRequestAsync(string email);
    Task<(bool Successo, string Messaggio)> ResetPasswordConfirmAsync(string token, string newPassword);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
}