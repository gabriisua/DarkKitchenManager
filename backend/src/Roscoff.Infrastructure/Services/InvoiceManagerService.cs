using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Invoice;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Invoice;
using Roscoff.Core.Enums;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class InvoiceManagerService : IInvoiceManagerService
{
    private readonly RoscoffDbContext _context;
    private readonly IFattureInCloudService _ficService;
    private readonly ILogger<InvoiceManagerService> _logger;

    public InvoiceManagerService(
        RoscoffDbContext context,
        IFattureInCloudService ficService,
        ILogger<InvoiceManagerService> logger)
    {
        _context = context;
        _ficService = ficService;
        _logger = logger;
    }

    // =========================================================================
    // --- METODI DI LETTURA / QUERY ---
    // =========================================================================

    public async Task<PaginatedResponseDto<PendingCustomerSummaryDto>> GetPendingInvoicesSummaryAsync(
        PendingInvoiceQueryParameters filter,
        CancellationToken cancellationToken = default)
    {
        var alreadyQueuedOrderIds = _context.PendingInvoices
            .Where(pi => pi.Status == PendingInvoiceStatus.Pending || pi.Status == PendingInvoiceStatus.Processing)
            .Select(pi => pi.OrderId);

        var query = _context.Orders
            .Where(o => (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped)
                        && o.FicInvoiceDocumentId == null
                        && !alreadyQueuedOrderIds.Contains(o.Id));

        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.CalculatedDeliveryDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(o => o.CalculatedDeliveryDate <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.Trim().ToLower();
            query = query.Where(o => o.Customer!.BusinessName.ToLower().Contains(searchTerm)
                                     || o.Customer.VatNumber.ToLower().Contains(searchTerm));
        }

        var flatQuery = query.Select(o => new
        {
            o.CustomerId,
            BusinessName = o.Customer!.BusinessName,
            VatNumber = o.Customer.VatNumber,
            o.NetAmountCents,
            o.VatAmountCents,
            o.TotalGrossCents,
            HasError = _context.PendingInvoices.Any(pi => pi.OrderId == o.Id && pi.Status == PendingInvoiceStatus.Failed)
        });

        var groupedQuery = flatQuery
            .GroupBy(x => new { x.CustomerId, x.BusinessName, x.VatNumber })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.BusinessName,
                g.Key.VatNumber,
                OrdersCount = g.Count(),
                NetAmountCents = g.Sum(x => x.NetAmountCents),
                VatAmountCents = g.Sum(x => x.VatAmountCents),
                TotalGrossCents = g.Sum(x => x.TotalGrossCents),
                HasFailedInvoices = g.Any(x => x.HasError)
            });

        var totalCount = await groupedQuery.CountAsync(cancellationToken);

        int validPage = filter.Page > 0 ? filter.Page : 1;
        int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var rawItems = await groupedQuery
            .OrderBy(c => c.BusinessName)
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync(cancellationToken);

        var items = rawItems.Select(x => new PendingCustomerSummaryDto(
            x.CustomerId, x.BusinessName, x.VatNumber, x.OrdersCount,
            x.NetAmountCents, x.VatAmountCents, x.TotalGrossCents,
            x.HasFailedInvoices
        )).ToList();

        return new PaginatedResponseDto<PendingCustomerSummaryDto>
        {
            Items = items, TotalCount = totalCount, Page = validPage, PageSize = validPageSize
        };
    }

    public async Task<List<PendingCustomerOrderNodeDto>> GetPendingOrdersByCustomerAsync(
        Guid customerId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken = default)
    {
        var alreadyQueuedOrderIds = _context.PendingInvoices
            .Where(pi => pi.Status == PendingInvoiceStatus.Pending || pi.Status == PendingInvoiceStatus.Processing)
            .Select(pi => pi.OrderId);

        var query = _context.Orders
            .Where(o => o.CustomerId == customerId
                        && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped)
                        && o.FicInvoiceDocumentId == null
                        && !alreadyQueuedOrderIds.Contains(o.Id))
            .AsNoTracking();

        if (dateFrom.HasValue) query = query.Where(o => o.CalculatedDeliveryDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(o => o.CalculatedDeliveryDate <= dateTo.Value);

        var rawOrders = await query
            .OrderBy(o => o.CalculatedDeliveryDate)
            .Select(o => new
            {
                o.Id, o.OrderNumber, o.OrderDate, o.CalculatedDeliveryDate, o.TotalGrossCents, o.CustomerReference,
                LatestError = _context.PendingInvoices
                    .Where(pi => pi.OrderId == o.Id && pi.Status == PendingInvoiceStatus.Failed)
                    .OrderByDescending(pi => pi.Id)
                    .Select(pi => pi.ErrorMessage)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return rawOrders.Select(o => new PendingCustomerOrderNodeDto(
            o.Id, o.OrderNumber, o.OrderDate, o.CalculatedDeliveryDate,
            o.TotalGrossCents, o.CustomerReference,
            o.LatestError
        )).ToList();
    }

    public async Task<PaginatedResponseDto<InvoiceHistorySummaryDto>> GetInvoicesHistoryAsync(
        InvoiceHistoryQueryParameters filter, 
        CancellationToken cancellationToken = default)
    {
        // 1. Query base sugli ordini già fatturati
        var query = _context.Orders
            .Include(o => o.Customer)
            .Where(o => o.FicInvoiceDocumentId != null);

        // 2. Filtri per Data
        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.CalculatedDeliveryDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(o => o.CalculatedDeliveryDate <= filter.DateTo.Value);

        // 3. Ricerca Globale (Cerca su Ragione Sociale, P.IVA o Numero Fattura)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.Trim().ToLower();
            query = query.Where(o => 
                (o.Customer != null && o.Customer.BusinessName.ToLower().Contains(searchTerm)) ||
                (o.Customer != null && o.Customer.VatNumber.ToLower().Contains(searchTerm)) ||
                (o.InvoiceNumber != null && o.InvoiceNumber.ToLower().Contains(searchTerm)));
        }

        // 4. Raggruppamento (Entity Framework compatibile)
        var groupedQuery = query
            .GroupBy(o => new 
            { 
                o.FicInvoiceDocumentId, 
                o.InvoiceNumber, 
                BusinessName = o.Customer!.BusinessName 
            })
            .Select(g => new 
            {
                FicDocumentId = g.Key.FicInvoiceDocumentId,
                InvoiceNumber = g.Key.InvoiceNumber,
                CustomerName = g.Key.BusinessName,
                OrdersCount = g.Count(),
                TotalGrossCents = g.Sum(o => o.TotalGrossCents),
                MaxDeliveryDate = g.Max(o => o.CalculatedDeliveryDate) 
            });

        // 5. Contiamo i record raggruppati per la paginazione
        var totalCount = await groupedQuery.CountAsync(cancellationToken);

        // 6. Ordinamento Dinamico
        bool isDesc = filter.SortDirection?.ToLower() != "asc";
        var sortCol = filter.SortColumn?.ToLower();

        groupedQuery = sortCol switch
        {
            "customer" => isDesc ? groupedQuery.OrderByDescending(x => x.CustomerName) : groupedQuery.OrderBy(x => x.CustomerName),
            "invoice"  => isDesc ? groupedQuery.OrderByDescending(x => x.InvoiceNumber) : groupedQuery.OrderBy(x => x.InvoiceNumber),
            "total"    => isDesc ? groupedQuery.OrderByDescending(x => x.TotalGrossCents) : groupedQuery.OrderBy(x => x.TotalGrossCents),
            "date"     => isDesc ? groupedQuery.OrderByDescending(x => x.MaxDeliveryDate) : groupedQuery.OrderBy(x => x.MaxDeliveryDate),
            _          => groupedQuery.OrderByDescending(x => x.MaxDeliveryDate) // Ordinamento di default
        };

        // 7. Paginazione e Recupero dati dal DB
        int validPage = filter.Page > 0 ? filter.Page : 1;
        int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var rawItems = await groupedQuery
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync(cancellationToken);

        // 8. Mapping in memoria nel record C#
        var items = rawItems.Select(x => new InvoiceHistorySummaryDto(
            x.FicDocumentId!.Value,
            x.InvoiceNumber ?? "Bozza",
            x.CustomerName,
            x.OrdersCount,
            x.TotalGrossCents,
            x.MaxDeliveryDate
        )).ToList();

        return new PaginatedResponseDto<InvoiceHistorySummaryDto>
        {
            Items = items, 
            TotalCount = totalCount, 
            Page = validPage, 
            PageSize = validPageSize
        };
    }

    // =========================================================================
    // --- METODI DI AZIONE / COMMAND ---
    // =========================================================================

    public async Task<(bool Success, string Message)> DeleteInvoiceAsync(int ficDocumentId, CancellationToken cancellationToken = default)
    {
        var ordersToReset = await _context.Orders
            .Where(o => o.FicInvoiceDocumentId == ficDocumentId)
            .ToListAsync(cancellationToken);

        if (!ordersToReset.Any())
            return (false, "Nessun ordine trovato nel gestionale associato a questa fattura.");

        try
        {
            await _ficService.DeleteInvoiceAsync(ficDocumentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore eliminazione fattura {DocId} su FiC", ficDocumentId);
            return (false, "Impossibile eliminare la fattura su Fatture in Cloud. Potrebbe essere già stata inviata allo SdI.");
        }

        foreach (var order in ordersToReset)
        {
            order.FicInvoiceDocumentId = null;
            order.InvoiceNumber = null;
        }

        var orderIds = ordersToReset.Select(o => o.Id).ToList();
        var pendingToClear = await _context.PendingInvoices
            .Where(pi => orderIds.Contains(pi.OrderId))
            .ToListAsync(cancellationToken);
            
        _context.PendingInvoices.RemoveRange(pendingToClear);

        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Fattura eliminata con successo. Gli ordini sono di nuovo disponibili per la fatturazione.");
    }
    
    public async Task<(bool Success, string? Url, string Message)> GetInvoiceUrlAsync(int ficDocumentId, CancellationToken cancellationToken = default)
    {
        try
        {
            string url = await _ficService.GetInvoiceUrlAsync(ficDocumentId, cancellationToken);

            return (true, url, "URL recuperato con successo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore recupero URL fattura {FicDocumentId}", ficDocumentId);
            return (false, null, $"DEBUG ERRORE: {ex.Message}");
        }
    }
}