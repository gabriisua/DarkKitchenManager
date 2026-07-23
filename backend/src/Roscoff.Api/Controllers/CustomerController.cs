using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers; 
using Roscoff.Core.Entities.Client;

namespace Roscoff.Api.Controllers; 

[Authorize(Roles = StaffRoles.Manager)] 
public class CustomerController : BaseApiController 
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CustomerQueryParameters parameters)
    {
        // Limite di sicurezza sulla paginazione per evitare sovraccarichi al DB
        if (parameters.PageSize > 100) parameters.PageSize = 100;
        if (parameters.PageSize < 1) parameters.PageSize = 10;
        if (parameters.Page < 1) parameters.Page = 1;

        var result = await _customerService.GetAllAsync(parameters);
        
        return HandleResult(Result<object>.Success(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        
        if (customer == null) 
            return HandleResult(Result<object>.Failure("Cliente non trovato."));

        return HandleResult(Result<object>.Success(customer));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerCreateDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var result = await _customerService.CreateAsync(request);
        
        if (!result.Success) 
            return HandleResult(Result<object>.Failure(result.Message));

        // Restituisce 201 Created mantenendo la struttura del wrapper per il frontend
        var wrapperResult = Result<object>.Success(new { result.CustomerId }, result.Message);
        return CreatedAtAction(nameof(GetById), new { id = result.CustomerId }, wrapperResult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateDto request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var result = await _customerService.UpdateAsync(id, request);
        
        if (!result.Success) 
            return HandleResult(Result<object>.Failure(result.Message));

        return HandleResult(Result<object>.Success(new { id }, result.Message));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);
        
        if (!result.Success) 
            return HandleResult(Result<object>.Failure(result.Message));

        return HandleResult(Result<object>.Success(new { id }, result.Message));
    }
}