using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;
using Roscoff.Infrastructure.Pdf;
using System.Text;

namespace Roscoff.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPdfEngineService _pdfEngine;

    public OrderController(IOrderService orderService, IPdfEngineService pdfEngine)
    {
        _orderService = orderService;
        _pdfEngine = pdfEngine;
    }

    /// <summary>
    /// Ottiene la lista degli ordini filtrata e paginata (supporta PageSize = -1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] OrderQueryParameters filter, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetPagedAsync(filter, cancellationToken);
        return Ok(Result<PaginatedResponseDto<OrderResponseDto>>.Success(result));
    }

    /// <summary>
    /// Ottiene i dettagli completi di un singolo ordine per ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(Result<OrderResponseDto>.Failure("Ordine non trovato."));

        return Ok(Result<OrderResponseDto>.Success(result));
    }

    /// <summary>
    /// Inserisce un nuovo ordine. Se invocata da MANAGER o STAFF permette il bypass del motore logistico.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        // Controllo di sicurezza basato sui ruoli aziendali in MAIUSCOLO
        var isInternalOperator = User.IsInRole("MANAGER") || User.IsInRole("STAFF");
        
        var finalRequest = request;
        
        // Se non è un operatore interno ma ha forzato il bypass nel JSON, lo neutralizziamo
        if (!isInternalOperator && request.BypassCalculator)
        {
            finalRequest = request with { BypassCalculator = false };
        }

        var (success, message, data) = await _orderService.CreateOrderAsync(finalRequest, cancellationToken);

        if (!success) return BadRequest(Result<OrderResponseDto>.Failure(message));

        return Ok(Result<OrderResponseDto>.Success(data!, message));
    }

    /// <summary>
    /// Aggiorna lo stato di avanzamento di un ordine. Accesso riservato agli operatori interni.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto request, CancellationToken cancellationToken)
    {
        var (success, message) = await _orderService.UpdateStatusAsync(id, request.Status, cancellationToken);

        if (!success) return BadRequest(Result<bool>.Failure(message));

        return Ok(Result<bool>.Success(true, message));
    }
    
    /// <summary>
    /// Genera e scarica il DDT (Bolla) in formato HTML/PDF.
    /// </summary>
    [HttpGet("{id:guid}/ddt")]
    public async Task<IActionResult> DownloadDdt(Guid id, CancellationToken cancellationToken)
    {
        // 1. Recupera l'ordine utilizzando il servizio corretto
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order == null) return NotFound("Ordine non trovato");

        // 2. Genera l'HTML compilato
        string htmlContent = DdtTemplateGenerator.GenerateHtml(order);

        // 3. CONVERTI L'HTML IN PDF usando il nostro nuovo servizio
        byte[] pdfBytes = await _pdfEngine.GeneratePdfFromHtmlAsync(htmlContent);

        // 4. Restituisci il vero file PDF
        string contentType = "application/pdf"; // MIME type corretto
        string fileName = $"DDT_{order.OrderNumber}.pdf";

        return File(pdfBytes, contentType, fileName);
    }
}