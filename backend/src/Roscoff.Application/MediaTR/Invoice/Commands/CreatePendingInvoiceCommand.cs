using MediatR;
using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Wrappers;
using Roscoff.Core.Entities.Invoice;
using Roscoff.Core.Enums;

namespace Roscoff.Application.MediaTR.Invoice.Commands;

// --- 1. MANCAVA QUESTO BLOCCO (IL COMANDO) ---
public class CreatePendingInvoicesCommand : IRequest<Result<int>>
{
    public List<Guid> OrderIds { get; set; } = new();
    public bool SendToSdiImmediately { get; set; } = false; 
}

// --- 2. IL TUO HANDLER ---
public class CreatePendingInvoicesCommandHandler : IRequestHandler<CreatePendingInvoicesCommand, Result<int>>
{
    private readonly IRoscoffDbContext _dbContext;

    public CreatePendingInvoicesCommandHandler(IRoscoffDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<int>> Handle(CreatePendingInvoicesCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderIds == null || !request.OrderIds.Any())
        {
            return Result<int>.Failure("Nessun ordine selezionato per la fatturazione.");
        }

        var validOrders = await _dbContext.Orders
            .Where(o => request.OrderIds.Contains(o.Id))
            .Where(o => o.FicInvoiceDocumentId == null) 
            .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped)
            .ToListAsync(cancellationToken);

        if (!validOrders.Any())
        {
            return Result<int>.Failure("Nessuno degli ordini selezionati è idoneo per la fatturazione (già fatturati o in stato non definitivo).");
        }

        var validOrderIds = validOrders.Select(o => o.Id).ToList();
        
        var alreadyQueuedOrderIds = await _dbContext.PendingInvoices
            .Where(p => validOrderIds.Contains(p.OrderId))
            .Where(p => p.Status == PendingInvoiceStatus.Pending || p.Status == PendingInvoiceStatus.Processing)
            .Select(p => p.OrderId)
            .ToListAsync(cancellationToken);

        var ordersToQueue = validOrders.Where(o => !alreadyQueuedOrderIds.Contains(o.Id)).ToList();

        if (!ordersToQueue.Any())
        {
            return Result<int>.Failure("Tutti gli ordini validi selezionati sono già in coda di elaborazione.");
        }

        var pendingInvoices = ordersToQueue.Select(order => new PendingInvoice
        {
            OrderId = order.Id,
            Status = PendingInvoiceStatus.Pending
        }).ToList();

        await _dbContext.PendingInvoices.AddRangeAsync(pendingInvoices, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(pendingInvoices.Count);
    }
}