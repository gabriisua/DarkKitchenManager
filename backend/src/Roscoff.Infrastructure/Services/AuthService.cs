using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Roscoff.Core.Entities.Client;
using Roscoff.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Roscoff.Application.Dtos.Auth;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;

namespace Roscoff.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly RoscoffDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(RoscoffDbContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var staff = await _context.StaffMembers
            .FirstOrDefaultAsync(s => s.Email == request.Email && s.IsActive);

        if (staff == null || !BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
            return null;

        staff.LastLogin = DateTime.UtcNow;
        
        // Genera JWT e Refresh Token
        var jwtToken = GenerateJwtToken(staff);
        var refreshToken = GenerateRefreshToken();

        // Salva il refresh token nel database
        staff.RefreshToken = refreshToken;
        staff.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Imposta la durata (es. 7 giorni)

        await _context.SaveChangesAsync();

        // Nota: Assicurati che LoginResponseDto accetti il nuovo parametro refreshToken
        return new LoginResponseDto(jwtToken, refreshToken, staff.Id, staff.Username, staff.Role);
    }

    // --- NUOVO METODO: Refresh Token ---
    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        // Cerca l'utente tramite il refresh token fornito
        var staff = await _context.StaffMembers
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && s.IsActive);

        // Verifica se l'utente esiste e se il token non è scaduto
        if (staff == null || staff.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            // Opzionale: potresti voler resettare il token se è scaduto per pulizia
            if (staff != null) 
            {
                staff.RefreshToken = null;
                staff.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync();
            }
            return null; 
        }

        // Genera nuovi token (Token Rotation per maggiore sicurezza)
        var newJwtToken = GenerateJwtToken(staff);
        var newRefreshToken = GenerateRefreshToken();

        // Aggiorna l'entità con il nuovo refresh token e resetta la scadenza
        staff.RefreshToken = newRefreshToken;
        staff.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return new LoginResponseDto(newJwtToken, newRefreshToken, staff.Id, staff.Username, staff.Role);
    }
    // ------------------------------------

    public async Task<(string Messaggio, string? Token)> ResetPasswordRequestAsync(string email)
    {
        var staff = await _context.StaffMembers.FirstOrDefaultAsync(s => s.Email == email && s.IsActive);
        if (staff == null)
            return ("Se l'email esiste nei nostri sistemi, riceverai un link per il reset.", null);

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        staff.ResetToken = token;
        staff.ResetTokenExpiry = DateTime.UtcNow.AddHours(2);

        await _context.SaveChangesAsync();

        return ("Richiesta generata con successo. Controlla la tua email.", token);
    }

    public async Task<(bool Successo, string Messaggio)> ResetPasswordConfirmAsync(string token, string newPassword)
    {
        var staff = await _context.StaffMembers.FirstOrDefaultAsync(s => s.ResetToken == token);
        
        if (staff == null || staff.ResetTokenExpiry < DateTime.UtcNow)
            return (false, "Token non valido o scaduto.");

        if (!IsPasswordValid(newPassword))
            return (false, "La nuova password non rispetta i requisiti minimi di sicurezza.");

        staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        staff.ResetToken = null;
        staff.ResetTokenExpiry = null;
        staff.PasswordResetAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "Password aggiornata con successo.");
    }

    private bool IsPasswordValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8) return false;
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    private string GenerateJwtToken(Staff staff)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, staff.Email ?? string.Empty),
            new Claim("Username", staff.Username),
            new Claim(ClaimTypes.Role, staff.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // --- NUOVO METODO: Generatore Refresh Token ---
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    // ----------------------------------------------
}