using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Interfaces;

namespace Roscoff.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PrintController : ControllerBase
{
    private readonly IPlateService _plateService;
    private readonly INutritionService _nutritionService;
    private readonly IPrinterService _printerService;

    public PrintController(
        IPlateService plateService, 
        INutritionService nutritionService,
        IPrinterService printerService)
    {
        _plateService = plateService;
        _nutritionService = nutritionService;
        _printerService = printerService;
    }

    // =========================================================================
    // 1. STANDARD
    // =========================================================================
    [HttpPost("standard/{id}/single")]
    public async Task<IActionResult> PrintStandardLabel(int id, [FromBody] PrintLabelRequestDto request)
    {
        return await ProcessSinglePrint(id, request, jobs => _printerService.PrintMultipleLabels(jobs), "Standard");
    }

    [HttpPost("standard/batch")]
    public async Task<IActionResult> PrintStandardBatch([FromBody] List<PrintBatchItemDto> requests)
    {
        return await ProcessBatchPrint(requests, jobs => _printerService.PrintMultipleLabels(jobs), "Standard");
    }

    // =========================================================================
    // 2. CORTILIA
    // =========================================================================
    [HttpPost("cortilia/{id}/single")]
    public async Task<IActionResult> PrintCortiliaLabel(int id, [FromBody] PrintLabelRequestDto request)
    {
        return await ProcessSinglePrint(id, request, jobs => _printerService.PrintCortiliaMultipleLabels(jobs), "Cortilia");
    }

    [HttpPost("cortilia/batch")]
    public async Task<IActionResult> PrintCortiliaBatch([FromBody] List<PrintBatchItemDto> requests)
    {
        return await ProcessBatchPrint(requests, jobs => _printerService.PrintCortiliaMultipleLabels(jobs), "Cortilia");
    }

    // =========================================================================
    // 3. FOORBAN
    // =========================================================================
    [HttpPost("foorban/{id}/single")]
    public async Task<IActionResult> PrintFoorbanLabel(int id, [FromBody] PrintLabelRequestDto request)
    {
        return await ProcessSinglePrint(id, request, jobs => _printerService.PrintFoorbanMultipleLabels(jobs), "Foorban");
    }

    [HttpPost("foorban/batch")]
    public async Task<IActionResult> PrintFoorbanBatch([FromBody] List<PrintBatchItemDto> requests)
    {
        return await ProcessBatchPrint(requests, jobs => _printerService.PrintFoorbanMultipleLabels(jobs), "Foorban");
    }

    // =========================================================================
    // 4. CRIOGENICO (NUOVI ENDPOINT)
    // =========================================================================
    [HttpPost("crio/{id}/single")]
    public async Task<IActionResult> PrintCrioLabel(int id, [FromBody] PrintLabelRequestDto request)
    {
        return await ProcessSinglePrint(id, request, jobs => _printerService.PrintCrioMultipleLabels(jobs), "Criogenico");
    }

    [HttpPost("crio/batch")]
    public async Task<IActionResult> PrintCrioBatch([FromBody] List<PrintBatchItemDto> requests)
    {
        return await ProcessBatchPrint(requests, jobs => _printerService.PrintCrioMultipleLabels(jobs), "Criogenico");
    }

    // =========================================================================
    // HELPER METHODS
    // =========================================================================
    private async Task<IActionResult> ProcessSinglePrint(int plateId, PrintLabelRequestDto request, Action<List<PrintJobRequestDto>> printAction, string formatName)
    {
        // PASSO ANCHE I NUOVI CAMPI CRIOGENICI
        var job = await BuildPrintJobAsync(
            plateId, request.Copies, request.PauseAfter, request.LotNumber, request.CustomExpiryDate, 
            request.IsWow, request.IsXl, request.CustomWeight, 
            request.IsThawed, request.ThawingDate, request.TargetLanguage);
        
        if (job == null) return NotFound(new { Message = $"Piatto con ID {plateId} non trovato." });

        try
        {
            printAction(new List<PrintJobRequestDto> { job });
            return Ok(new { Message = $"Inviate {request.Copies} etichette formato {formatName} alla stampante." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Impossibile comunicare con la stampante.", Detail = ex.Message });
        }
    }

    private async Task<IActionResult> ProcessBatchPrint(List<PrintBatchItemDto> requests, Action<List<PrintJobRequestDto>> printAction, string formatName)
    {
        if (requests == null || !requests.Any()) return BadRequest(new { Message = "Nessun lavoro di stampa fornito." });

        var printJobs = new List<PrintJobRequestDto>();

        foreach (var req in requests)
        {
            // PASSO ANCHE I NUOVI CAMPI CRIOGENICI
            var job = await BuildPrintJobAsync(
                req.PlateId, req.Copies, req.PauseAfter, req.LotNumber, req.CustomExpiryDate, 
                req.IsWow, req.IsXl, req.CustomWeight, 
                req.IsThawed, req.ThawingDate, req.TargetLanguage);
                
            if (job != null) printJobs.Add(job);
        }

        if (!printJobs.Any()) return BadRequest(new { Message = "Nessun piatto valido trovato nel lotto fornito." });

        try
        {
            printAction(printJobs);
            return Ok(new { Message = $"Inviati {printJobs.Count} lotti formato {formatName} in modo sequenziale." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Errore durante l'invio alla stampante.", Detail = ex.Message });
        }
    }

    // AGGIUNTI I CAMPI isThawed, thawingDate, targetLanguage NELLA FIRMA
    private async Task<PrintJobRequestDto?> BuildPrintJobAsync(
        int plateId, int copies, int pauseAfter, string? rawLotNumber, DateTime? customExpiry, 
        bool isWow, bool isXl, decimal? customWeight, 
        bool isThawed, DateTime? thawingDate, string? targetLanguage)
    {
        var plate = await _plateService.GetPlateWithIngredientsAsync(plateId);
        if (plate == null) return null;

        var nutrition = await _nutritionService.CalculatePlateNutritionAsync(plateId);
        string allergensInTraces = nutrition.Allergens != null && nutrition.Allergens.Any() 
            ? string.Join(", ", nutrition.Allergens.Select(a => a.Name)) 
            : "Nessuno";

        string lotNumber = string.IsNullOrWhiteSpace(rawLotNumber) ? $"L-{DateTime.Now:ddMM}" : rawLotNumber;

        return new PrintJobRequestDto
        {
            Plate = plate,
            Nutrition = nutrition,
            Allergens = allergensInTraces,
            LotNumber = lotNumber,
            ProductionDate = DateTime.Now,
            Copies = copies,
            PauseAfter = pauseAfter,
            CustomExpiryDate = customExpiry,
            IsWow = isWow,
            IsXl = isXl,
            CustomWeight = customWeight,
            IsThawed = isThawed,             // MAPPATO QUI
            ThawingDate = thawingDate,       // MAPPATO QUI
            TargetLanguage = targetLanguage  // MAPPATO QUI
        };
    }
}