using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Roscoff.Core.Interfaces;

namespace Roscoff.Api.HttpService;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Sistema";

        return user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? "Sistema";
    }
}