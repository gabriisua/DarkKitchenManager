using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;

namespace Roscoff.Api.Controllers;

public class IngredientController : BaseApiController
{
    private readonly IIngredientService _ingredientService;

    public IngredientController(IIngredientService ingredientService)
    {
        _ingredientService = ingredientService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngredientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var ingredient = await _ingredientService.CreateIngredientAsync(dto);

        // Prepariamo il wrapper di successo
        var wrapperResult = Result<IngredientResponseDto>.Success(ingredient, "Ingrediente creato con successo.");
        
        // Restituisce 201 Created con l'URL per recuperare la risorsa e il JSON strutturato
        return CreatedAtAction(nameof(GetById), new { id = ingredient.Id }, wrapperResult);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] IngredientQueryParameters parameters)
    {
        if (parameters.PageSize > 100) parameters.PageSize = 100;
        if (parameters.Page < 1) parameters.Page = 1;

        var result = await _ingredientService.GetAllAsync(parameters);
        return HandleResult(Result<PaginatedResponseDto<IngredientResponseDto>>.Success(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
        
        if (ingredient == null)
            return NotFound(Result<IngredientResponseDto>.Failure($"Ingrediente con ID {id} non trovato."));

        return HandleResult(Result<IngredientResponseDto>.Success(ingredient));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIngredientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var updatedIngredient = await _ingredientService.UpdateIngredientAsync(id, dto);

        if (updatedIngredient == null)
            return NotFound(Result<IngredientResponseDto>.Failure($"Ingrediente con ID {id} non trovato. Impossibile aggiornare."));

        return HandleResult(Result<IngredientResponseDto>.Success(updatedIngredient, "Ingrediente aggiornato con successo."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var success = await _ingredientService.SoftDeleteIngredientAsync(id);

        if (!success)
            return NotFound(Result<object>.Failure($"Ingrediente con ID {id} non trovato."));

        // Usiamo un bool o un object vuoto per il payload di una cancellazione riuscita
        return HandleResult(Result<bool>.Success(true, "Ingrediente eliminato con successo."));
    }
}