using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;
using Roscoff.Infrastructure.Pdf; // Namespace dove risiede il generatore di template

namespace Roscoff.Api.Controllers;

public class MenuController : BaseApiController
{
    private readonly IMenuService _menuService;
    private readonly IPdfEngineService _pdfEngineService; // INIETTATO

    public MenuController(IMenuService menuService, IPdfEngineService pdfEngineService)
    {
        _menuService = menuService;
        _pdfEngineService = pdfEngineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MenuQueryParameters parameters)
    {
        if (parameters.PageSize > 100) parameters.PageSize = 100;
        if (parameters.Page < 1) parameters.Page = 1;

        var result = await _menuService.GetAllAsync(parameters);
        return HandleResult(Result<PaginatedResponseDto<MenuResponseDto>>.Success(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var menu = await _menuService.GetByIdAsync(id);

        if (menu == null)
            return HandleResult(Result<MenuResponseDto>.Failure($"Menu con ID {id} non trovato."));

        return HandleResult(Result<MenuResponseDto>.Success(menu));
    }

    // NUOVO ENDPOINT PER IL PDF
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        // 1. Recuperiamo il menu tramite il servizio (accertati che la GetById includa Items -> Plate -> Category)
        var menu = await _menuService.GetByIdAsync(id);

        if (menu == null)
            return HandleResult(Result<MenuResponseDto>.Failure($"Menu con ID {id} non trovato. Impossibile generare il PDF."));

        // 2. Generiamo l'HTML passando il DTO invece dell'entità
        string htmlContent = MenuTemplateGenerator.GenerateHtml(menu);

        // 3. Convertiamo l'HTML in array di byte PDF tramite Puppeteer
        byte[] pdfBytes = await _pdfEngineService.GeneratePdfFromHtmlAsync(htmlContent);

        // 4. Prepariamo il nome del file e restituiamo il file binario
        // Nota: Qui non usiamo HandleResult perché dobbiamo restituire un file stream, non un JSON
        string safeFileName = $"{menu.Name.Replace(" ", "_")}_Menu.pdf";
        return File(pdfBytes, "application/pdf", safeFileName);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MenuCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var menu = await _menuService.CreateAsync(dto);
        var wrapperResult = Result<MenuResponseDto>.Success(menu, "Menu creato con successo.");

        return CreatedAtAction(nameof(GetById), new { id = menu.Id }, wrapperResult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MenuUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(Result<object>.Failure("Dati non validi."));

        var updatedMenu = await _menuService.UpdateAsync(id, dto);

        if (updatedMenu == null)
            return HandleResult(Result<MenuResponseDto>.Failure($"Menu con ID {id} non trovato. Impossibile aggiornare."));

        return HandleResult(Result<MenuResponseDto>.Success(updatedMenu, "Menu updatedMenu successo."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _menuService.DeleteAsync(id);

        if (!success)
            return HandleResult(Result<object>.Failure($"Menu con ID {id} non trovato."));

        return HandleResult(Result<bool>.Success(true, "Menu eliminato con successo."));
    }
}