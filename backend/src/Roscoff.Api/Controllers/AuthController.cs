using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Auth;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers; 

namespace Roscoff.Api.Controllers;

public class AuthController : BaseApiController 
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return HandleResult(Result<LoginResponseDto>.Failure("Email e password sono obbligatori."));
        }

        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return HandleResult(Result<LoginResponseDto>.Failure("Credenziali non valide o utente disabilitato."));
        }

        return HandleResult(Result<LoginResponseDto>.Success(response));
    }
    
    [HttpPost("reset-password-request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return HandleResult(Result<object>.Failure("L'email è obbligatoria."));
        }

        var (messaggio, token) = await _authService.ResetPasswordRequestAsync(request.Email);
        
        // ATTENZIONE: In produzione invia il token via email, non restituirlo nell'API!
        return HandleResult(Result<object>.Success(new { message = messaggio, debugToken = token }));
    }

    [HttpPost("reset-password-confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ResetPasswordConfirmDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return HandleResult(Result<object>.Failure("Token e Nuova Password sono obbligatori."));
        }

        var (successo, messaggio) = await _authService.ResetPasswordConfirmAsync(request.Token, request.NewPassword);

        if (!successo)
        {
            return HandleResult(Result<object>.Failure(messaggio));
        }

        return HandleResult(Result<object>.Success(new { message = messaggio }));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // Sfruttiamo le proprietà ereditate dal BaseApiController per un codice pulitissimo
        var userInfo = new
        {
            Id = AuthenticatedUserId,
            Email = AuthenticatedUserEmail,
            Role = AuthenticatedUserRole
        };

        return HandleResult(Result<object>.Success(userInfo));
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
    
        if (response == null)
            return Unauthorized("Refresh token non valido o scaduto.");
        
        return Ok(response);
    }
}