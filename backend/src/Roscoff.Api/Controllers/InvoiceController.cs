using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Invoice;
using Roscoff.Application.Interfaces;
using Roscoff.Application.MediaTR.Invoice.Commands;
using Roscoff.Application.Wrappers; 

namespace Roscoff.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "MANAGER")] 
public class InvoiceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IInvoiceManagerService _invoiceManagerService;

    // Iniettiamo sia MediatR per i comandi asincroni, sia il ManagerService per letture e operazioni dirette
    public InvoiceController(IMediator mediator, IInvoiceManagerService invoiceManagerService)
    {
        _mediator = mediator;
        _invoiceManagerService = invoiceManagerService;
    }

    // 1. POST: api/invoice/bulk-invoice (Avvia la coda in background)
    [HttpPost("bulk-invoice")]
    public async Task<IActionResult> CreateBulkInvoices([FromBody] CreatePendingInvoicesCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded) 
        {
            return BadRequest(result);
        }

        return Accepted(result); 
    }

    // 2. GET: api/invoice/pending-summary (Elenco clienti con totali ordini da fatturare)
    [HttpGet("pending-summary")]
    public async Task<IActionResult> GetPendingSummary([FromQuery] PendingInvoiceQueryParameters filter, CancellationToken cancellationToken)
    {
        var result = await _invoiceManagerService.GetPendingInvoicesSummaryAsync(filter, cancellationToken);
        return Ok(Result<PaginatedResponseDto<PendingCustomerSummaryDto>>.Success(result));
    }

    // 3. GET: api/invoice/pending-summary/{customerId}/orders (Dettaglio ordini del singolo cliente)
    [HttpGet("pending-summary/{customerId}/orders")]
    public async Task<IActionResult> GetPendingOrders(Guid customerId, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, CancellationToken cancellationToken)
    {
        var result = await _invoiceManagerService.GetPendingOrdersByCustomerAsync(customerId, dateFrom, dateTo, cancellationToken);
        return Ok(Result<List<PendingCustomerOrderNodeDto>>.Success(result));
    }
    
    // 4. GET: api/invoice/history (Elenco storico delle fatture già generate con filtri completi)
    [HttpGet("history")]
    public async Task<IActionResult> GetInvoicesHistory([FromQuery] InvoiceHistoryQueryParameters filter, CancellationToken cancellationToken)
    {
        var result = await _invoiceManagerService.GetInvoicesHistoryAsync(filter, cancellationToken);
        return Ok(Result<PaginatedResponseDto<InvoiceHistorySummaryDto>>.Success(result));
    }

    // 5. DELETE: api/invoice/{ficDocumentId} (Elimina fattura da FiC e ripristina ordini nel gestionale)
    [HttpDelete("{ficDocumentId:int}")]
    [Authorize(Roles = "MANAGER,ADMIN")] // Manteniamo il controllo rigido sulle eliminazioni
    public async Task<IActionResult> DeleteInvoice(int ficDocumentId, CancellationToken cancellationToken)
    {
        var (success, message) = await _invoiceManagerService.DeleteInvoiceAsync(ficDocumentId, cancellationToken);

        if (!success) 
        {
            return BadRequest(Result<bool>.Failure(message));
        }

        return Ok(Result<bool>.Success(true, message));
    }

    // 6. GET: api/invoice/{ficDocumentId}/pdf (Recupera l'URL pubblico del PDF della fattura)
    [HttpGet("{ficDocumentId:int}/pdf")]
    public async Task<IActionResult> GetInvoicePdfUrl(int ficDocumentId, CancellationToken cancellationToken)
    {
        var (success, invoiceUrl, message) = await _invoiceManagerService.GetInvoiceUrlAsync(ficDocumentId, cancellationToken);

        if (!success || string.IsNullOrEmpty(invoiceUrl))
        {
            // Restituiamo il BadRequest wrappato per informare Angular dell'errore
            return BadRequest(Result<string>.Failure(message));
        }

        // Restituiamo il link come stringa al frontend. 
        // Angular potrà fare window.open(res.data, '_blank') per aprirlo!
        return Ok(Result<string>.Success(invoiceUrl, message));
    }
}