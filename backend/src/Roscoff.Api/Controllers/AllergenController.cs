using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Wrappers;
using Roscoff.Core.Entities.Client;

namespace Roscoff.Api.Controllers;

[Authorize(Roles = StaffRoles.Manager)] 
public class AllergenController : BaseApiController
{
    private readonly IAllergenService _allergenService;

    public AllergenController(IAllergenService allergenService)
    {
        _allergenService = allergenService;
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAllAllergens(CancellationToken cancellationToken)
    {
        var result = await _allergenService.GetAllAllergensAsync(cancellationToken);
        
        // Assumendo che tu stia usando il tuo solito wrapper Result<T>
        return HandleResult(Result<IEnumerable<AllergenResponseDto>>.Success(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AllergenQueryParameters parameters)
    {
        if (parameters.PageSize > 100) parameters.PageSize = 100;
        if (parameters.Page < 1) parameters.Page = 1;

        var result = await _allergenService.GetAllAsync(parameters);
        return HandleResult(Result<PaginatedResponseDto<AllergenResponseDto>>.Success(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var allergen = await _allergenService.GetByIdAsync(id);
        
        if (allergen == null) 
            return HandleResult(Result<object>.Failure("Allergene non trovato."));

        return HandleResult(Result<object>.Success(allergen));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AllergenCreateDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var result = await _allergenService.CreateAsync(request);
        
        if (!result.Success) 
            return HandleResult(Result<object>.Failure(result.Message));

        var wrapperResult = Result<object>.Success(new { result.AllergenId }, result.Message);
        return CreatedAtAction(nameof(GetById), new { id = result.AllergenId }, wrapperResult);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AllergenUpdateDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var result = await _allergenService.UpdateAsync(id, request);
        
        if (!result.Success) 
            return HandleResult(Result<object>.Failure(result.Message));

        return HandleResult(Result<object>.Success(new { id }, result.Message));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _allergenService.DeleteAsync(id);
        
        if (!result.Success) 
            return HandleResult(Result<object>.Failure(result.Message));

        return HandleResult(Result<object>.Success(new { id }, result.Message));
    }
}