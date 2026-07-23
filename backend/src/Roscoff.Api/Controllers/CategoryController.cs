using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers; // Namespace dove tieni il wrapper Result<T>

namespace Roscoff.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Richiede autenticazione per tutti gli endpoint
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Ottiene tutte le categorie attive (senza paginazione).
    /// Utile per popolare le tendine (dropdown) nei form.
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllActiveAsync(cancellationToken);
        return Ok(Result<IEnumerable<CategoryReadDto>>.Success(result));
    }

    /// <summary>
    /// Ottiene la lista paginata delle categorie con supporto alla ricerca.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null, 
        CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetPagedAsync(page, pageSize, search, cancellationToken);
        return Ok(Result<PaginatedResponseDto<CategoryReadDto>>.Success(result));
    }

    /// <summary>
    /// Recupera i dettagli di una singola categoria tramite ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        
        if (result == null)
            return NotFound(Result<CategoryReadDto>.Failure("Categoria non trovata."));

        return Ok(Result<CategoryReadDto>.Success(result));
    }

    /// <summary>
    /// Crea una nuova categoria.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "MANAGER")] // Solo i Manager possono creare categorie
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto request, CancellationToken cancellationToken)
    {
        var (success, message, categoryId) = await _categoryService.CreateAsync(request, cancellationToken);
        
        if (!success)
            return BadRequest(Result<int?>.Failure(message));

        return Ok(Result<int?>.Success(categoryId, message));
    }

    /// <summary>
    /// Aggiorna una categoria esistente.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "MANAGER")] // Solo i Manager possono modificare
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto request, CancellationToken cancellationToken)
    {
        var (success, message) = await _categoryService.UpdateAsync(id, request, cancellationToken);
        
        if (!success)
            return BadRequest(Result<bool>.Failure(message));

        return Ok(Result<bool>.Success(true, message));
    }

    /// <summary>
    /// Elimina definitivamente una categoria (se non ha piatti associati).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "MANAGER")] // Solo i Manager possono eliminare
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var (success, message) = await _categoryService.DeleteAsync(id, cancellationToken);
        
        if (!success)
            return BadRequest(Result<bool>.Failure(message));

        return Ok(Result<bool>.Success(true, message));
    }
}