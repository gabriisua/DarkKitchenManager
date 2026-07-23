using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Roscoff.Application.Wrappers;

namespace Roscoff.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // =========================================================
    // GESTIONE STANDARDIZZATA DELLE RISPOSTE (WRAPPER)
    // =========================================================

    /// <summary>
    /// Traduce un oggetto Result nel corrispondente IActionResult (HTTP Status Code)
    /// mantenendo invariata la struttura JSON di risposta.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null)
        {
            return NotFound(new Result<string> { Succeeded = false, Message = "Risorsa non trovata." });
        }

        if (result.Succeeded)
        {
            // Operazione completata con successo: HTTP 200
            return Ok(result);
        }

        // Se l'operazione è fallita e il messaggio suggerisce l'assenza della risorsa: HTTP 404
        if (!string.IsNullOrWhiteSpace(result.Message) && 
            result.Message.Contains("non trovat", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        // Di default per tutti gli altri errori di validazione o logica: HTTP 400
        return BadRequest(result);
    }

    // =========================================================
    // ESTRAZIONE DATI UTENTE AUTENTICATO (JWT)
    // =========================================================

    protected Guid AuthenticatedUserId
    {
        get
        {
            var authenticatedUserId = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            
            // Nota: Se nel DB i tuoi utenti usano un ID di tipo 'int' (es. BaseEntity<int>),
            // potresti dover cambiare Guid.TryParse con int.TryParse in futuro!
            if (Guid.TryParse(authenticatedUserId, out Guid userId))
            {
                return userId;
            }
            else
            {
                return Guid.Empty;
            }
        }
    }

    protected string? AuthenticatedUserRole
    {
        get
        {
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            return userRole;
        }
    }

    protected string? AuthenticatedUserEmail
    {
        get
        {
            var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            return userEmail;
        }
    }
}