namespace Roscoff.Application.Dtos.Auth;

public record LoginRequestDto(string Email, string Password);

// Modificato: aggiunto 'string RefreshToken' come secondo parametro
public record LoginResponseDto(string Token, string RefreshToken, Guid Id, string Username, string Role);

// Nuovo: DTO specifico per la rotta di refresh del token (es. POST /api/auth/refresh)
public record RefreshTokenRequestDto(string RefreshToken);

public record ResetPasswordRequestDto(string Email);
public record ResetPasswordConfirmDto(string Token, string NewPassword);