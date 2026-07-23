using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers; 

namespace Roscoff.Api.Controllers;

[Authorize]
public class StaffController : BaseApiController 
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StaffQueryParameters parameters)
    {
        if (parameters.PageSize > 100) 
            parameters.PageSize = 100;
        
        if (parameters.Page < 1) 
            parameters.Page = 1;
        
        var result = await _staffService.GetAllAsync(parameters);
    
        return HandleResult(Result<PaginatedResponseDto<StaffReadDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var staff = await _staffService.GetByIdAsync(id);
        
        if (staff == null) 
            return HandleResult(Result<StaffReadDto>.Failure("Membro dello staff non trovato."));
        
        return HandleResult(Result<StaffReadDto>.Success(staff));
    }

    [HttpPost]
    public async Task<IActionResult> Create(StaffCreateDto dto)
    {
        try
        {
            var result = await _staffService.CreateAsync(dto);
            
            // Prepariamo il wrapper di successo
            var wrapperResult = Result<StaffReadDto>.Success(result, "Membro dello staff creato con successo.");
            
            // Restituisce 201 Created mantenendo la struttura
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, wrapperResult);
        }
        catch (ArgumentException ex)
        {
            // Intercetta l'eccezione e restituisce un 400 Bad Request formattato
            return BadRequest(Result<object>.Failure(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, StaffUpdateDto dto)
    {
        var success = await _staffService.UpdateAsync(id, dto);
        
        if (!success) 
            return HandleResult(Result<object>.Failure("Membro dello staff non trovato o impossibile aggiornare."));
        
        return HandleResult(Result<object>.Success(new { id }, "Staff aggiornato con successo."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _staffService.SoftDeleteAsync(id);
        
        if (!success) 
            return HandleResult(Result<object>.Failure("Membro dello staff non trovato o già eliminato."));
        
        // Passiamo a Success invece di NoContent per restituire il wrapper JSON al frontend
        return HandleResult(Result<object>.Success(new { id }, "Staff eliminato con successo."));
    }
}