using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;
using Roscoff.Core.Entities.Client;

namespace Roscoff.Api.Controllers;

[Authorize(Roles = StaffRoles.Manager)] 
[Route("api/customers/{customerId:guid}/hubs")]
public class DeliveryHubsController : BaseApiController
{
    private readonly IDeliveryHubService _hubService;

    public DeliveryHubsController(IDeliveryHubService hubService)
    {
        _hubService = hubService;
    }

    [HttpGet] // <-- Nuovo Endpoint
    public async Task<IActionResult> GetByCustomerId(Guid customerId)
    {
        var hubs = await _hubService.GetByCustomerIdAsync(customerId);
        
        // Ritorna la lista formattata secondo lo standard del tuo frontend wrapper
        return HandleResult(Result<object>.Success(hubs));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid customerId, [FromBody] DeliveryHubCreateDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var result = await _hubService.CreateAsync(customerId, request);

        if (!result.Success)
            return HandleResult(Result<object>.Failure(result.Message));

        return HandleResult(Result<object>.Success(new { HubId = result.HubId }, result.Message));
    }

    [HttpPut("{hubId:guid}")]
    public async Task<IActionResult> Update(Guid customerId, Guid hubId, [FromBody] DeliveryHubUpdateDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var result = await _hubService.UpdateAsync(customerId, hubId, request);

        if (!result.Success)
            return HandleResult(Result<object>.Failure(result.Message));

        return HandleResult(Result<object>.Success(new { id = hubId }, result.Message));
    }

    [HttpDelete("{hubId:guid}")]
    public async Task<IActionResult> Delete(Guid customerId, Guid hubId)
    {
        var result = await _hubService.DeleteAsync(customerId, hubId);

        if (!result.Success)
            return HandleResult(Result<object>.Failure(result.Message));

        return HandleResult(Result<object>.Success(new { id = hubId }, result.Message));
    }
}