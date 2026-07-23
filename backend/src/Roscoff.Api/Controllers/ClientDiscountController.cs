using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;
using Roscoff.Application.MediaTR.Client.Queries;
using Roscoff.Core.Entities.Client;
using Roscoff.Application.Dtos.Common; // Aggiunto per DiscountQueryParameters e PaginatedResponseDto
using Roscoff.Application.Dtos.Client;
using Roscoff.Application.MediaTR.Discount.Commands; // Aggiunto per PlateDiscountDto e CategoryDiscountDto

namespace Roscoff.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Nota: se vuoi limitarlo, usa [Authorize(Roles = "Manager")]
public class ClientDiscountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IClientDiscountService _discountService;

    public ClientDiscountController(IMediator mediator, IClientDiscountService discountService)
    {
        _mediator = mediator;
        _discountService = discountService;
    }

    // ==========================================
    // --- MOTORE PREZZI ---
    // ==========================================

    [HttpGet("customers/{customerId:guid}/plates/{plateId:int}/effective-price")]
    public async Task<IActionResult> GetEffectivePrice(Guid customerId, int plateId, CancellationToken cancellationToken)
    {
        try 
        {
            var price = await _discountService.GetEffectivePriceAsync(customerId, plateId, cancellationToken);
            return Ok(Result<int>.Success(price, "Prezzo calcolato con successo."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Result<int>.Failure("Piatto non trovato nel catalogo.", new List<string> { ex.Message }));
        }
    }

    // ==========================================
    // --- GESTIONE SCONTI (CLIENTE SPECIFICO) ---
    // ==========================================

    [HttpGet("customers/{customerId:guid}/categories")]
    public async Task<IActionResult> GetCategoryDiscounts(Guid customerId)
    {
        var result = await _mediator.Send(new GetCategoryDiscountsQuery(customerId));
        return Ok(Result<List<ClientCategoryDiscount>>.Success(result)); 
    }

    [HttpPost("customers/{customerId:guid}/categories")]
    public async Task<IActionResult> SetCategoryDiscount(Guid customerId, [FromBody] SetCategoryDiscountCommand command)
    {
        command.CustomerId = customerId; 
        var success = await _mediator.Send(command);
        
        if (success)
            return Ok(Result<bool>.Success(true, "Sconto di categoria salvato correttamente."));
            
        return BadRequest(Result<bool>.Failure("Errore durante il salvataggio dello sconto di categoria."));
    }

    [HttpGet("customers/{customerId:guid}/plates")]
    public async Task<IActionResult> GetPlateDiscounts(Guid customerId)
    {
        var result = await _mediator.Send(new GetPlateDiscountsQuery(customerId));
        return Ok(Result<List<ClientPlateDiscount>>.Success(result));
    }

    [HttpPost("customers/{customerId:guid}/plates")]
    public async Task<IActionResult> SetPlateDiscount(Guid customerId, [FromBody] SetPlateDiscountCommand command)
    {
        command.CustomerId = customerId;
        var success = await _mediator.Send(command);
        
        if (success)
            return Ok(Result<bool>.Success(true, "Sconto del piatto salvato correttamente."));
            
        return BadRequest(Result<bool>.Failure("Errore durante il salvataggio dello sconto del piatto."));
    }

    // ==========================================
    // --- NUOVI ENDPOINT: GRID GLOBALI (PAGINATE) ---
    // ==========================================

    [HttpGet("plates/paged")]
    public async Task<IActionResult> GetPagedPlateDiscounts([FromQuery] DiscountQueryParameters parameters)
    {
        var result = await _discountService.GetPagedPlateDiscountsAsync(parameters);
        return Ok(Result<PaginatedResponseDto<PlateDiscountDto>>.Success(result));
    }

    [HttpGet("categories/paged")]
    public async Task<IActionResult> GetPagedCategoryDiscounts([FromQuery] DiscountQueryParameters parameters)
    {
        var result = await _discountService.GetPagedCategoryDiscountsAsync(parameters);
        return Ok(Result<PaginatedResponseDto<CategoryDiscountDto>>.Success(result));
    }

    // ==========================================
    // --- NUOVI ENDPOINT: HARD DELETE ---
    // ==========================================

    [HttpDelete("customers/{customerId:guid}/plates/{plateId:int}")]
    public async Task<IActionResult> DeletePlateDiscount(Guid customerId, int plateId)
    {
        var (success, message) = await _discountService.DeletePlateDiscountAsync(customerId, plateId);
        
        if (success)
            return Ok(Result<bool>.Success(true, message));
            
        return BadRequest(Result<bool>.Failure(message));
    }

    [HttpDelete("customers/{customerId:guid}/categories/{categoryId:int}")]
    public async Task<IActionResult> DeleteCategoryDiscount(Guid customerId, int categoryId)
    {
        var (success, message) = await _discountService.DeleteCategoryDiscountAsync(customerId, categoryId);
        
        if (success)
            return Ok(Result<bool>.Success(true, message));
            
        return BadRequest(Result<bool>.Failure(message));
    }
}