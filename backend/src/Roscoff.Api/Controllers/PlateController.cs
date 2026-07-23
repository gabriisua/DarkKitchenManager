using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;

namespace Roscoff.Api.Controllers;

public class PlateController : BaseApiController
{
    private readonly IPlateService _plateService;
    private readonly IFoodCostService _foodCostService;
    private readonly INutritionService _nutritionService;
    private readonly IPdfEngineService _pdfEngineService;
    
    public PlateController(
        IPlateService plateService, 
        IFoodCostService foodCostService, 
        INutritionService nutritionService,
        IPdfEngineService pdfEngineService) 
    {
        _plateService = plateService;
        _foodCostService = foodCostService;
        _nutritionService = nutritionService; 
        _pdfEngineService = pdfEngineService;
    }

    #region CRUD Operations

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PlateQueryParameters parameters)
    {
        if (parameters.PageSize > 100) parameters.PageSize = 100;
        if (parameters.Page < 1) parameters.Page = 1;

        var result = await _plateService.GetAllAsync(parameters);
        return HandleResult(Result<PaginatedResponseDto<PlateResponseDto>>.Success(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestPlateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var plate = await _plateService.CreatePlateAsync(dto);
        var wrapperResult = Result<PlateResponseDto>.Success(plate, "Piatto creato con successo.");
        
        return CreatedAtAction(nameof(GetById), new { id = plate.Id }, wrapperResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plate = await _plateService.GetPlateWithIngredientsAsync(id);

        if (plate == null)
            return HandleResult(Result<PlateResponseDto>.Failure($"Piatto con ID {id} non trovato."));

        return HandleResult(Result<PlateResponseDto>.Success(plate));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var updatedPlate = await _plateService.UpdatePlateAsync(id, dto);

        if (updatedPlate == null)
            return HandleResult(Result<PlateResponseDto>.Failure($"Piatto con ID {id} non trovato. Impossibile aggiornare."));

        return HandleResult(Result<PlateResponseDto>.Success(updatedPlate, "Piatto aggiornato con successo."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var success = await _plateService.SoftDeletePlateAsync(id);

        if (!success)
            return HandleResult(Result<object>.Failure($"Piatto con ID {id} non trovato."));

        return HandleResult(Result<bool>.Success(true, "Piatto eliminato con successo."));
    }

    #endregion

    #region Analytical Engines (Food Cost & Nutrition & PDF)

    [HttpGet("{id}/food-cost")]
    public async Task<IActionResult> GetPlateFoodCost(int id)
    {
        var costInCents = await _foodCostService.CalculatePlateFoodCostAsync(id);
        
        if (costInCents == 0)
            return HandleResult(Result<object>.Failure($"Nessun ingrediente trovato o piatto inesistente per l'ID {id}."));

        var responseData = new 
        { 
            PlateId = id, 
            FoodCostCents = costInCents,
            FormattedDisplay = $"€ {costInCents / 100.0:0.00}"
        };

        return HandleResult(Result<object>.Success(responseData));
    }

    [HttpGet("{id}/nutrition")]
    public async Task<IActionResult> GetPlateNutrition(int id)
    {
        var nutritionSummary = await _nutritionService.CalculatePlateNutritionAsync(id);
        
        if (nutritionSummary.TotalWeightGrams == 0)
            return HandleResult(Result<NutritionalSummaryDto>.Failure($"Impossibile calcolare i valori nutrizionali. Piatto vuoto o inesistente per l'ID {id}."));

        return HandleResult(Result<NutritionalSummaryDto>.Success(nutritionSummary));
    }
    
    [HttpGet("{id}/technical-sheet")]
    public async Task<IActionResult> DownloadTechnicalSheet(int id)
    {
        var fileBytes = await _plateService.GenerateTechnicalSheetPdfAsync(id);

        if (fileBytes == null)
            return NotFound(new { Message = "Piatto non trovato o impossibile generare la scheda tecnica." });

        return File(fileBytes, "application/pdf", $"Scheda_Tecnica_{id}.pdf");
    }

    #endregion
}