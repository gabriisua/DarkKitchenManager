using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Invoice;
using Roscoff.Core.Enums;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly RoscoffDbContext _context;
    private readonly IWorkingDayCalculator _workingDayCalculator;
    private readonly IClientDiscountService _discountService; // <-- Aggiunto il motore prezzi

    public OrderService(RoscoffDbContext context, IWorkingDayCalculator workingDayCalculator, IClientDiscountService discountService)
    {
        _context = context;
        _workingDayCalculator = workingDayCalculator;
        _discountService = discountService;
    }

    public async Task<(bool Success, string Message, OrderResponseDto? Data)> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1. Verifiche base
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.IsActive, cancellationToken);
        if (customer == null) return (false, "Cliente non trovato o inattivo.", null);

        var hub = await _context.DeliveryHubs.FirstOrDefaultAsync(h => h.Id == request.DeliveryHubId && h.CustomerId == request.CustomerId && h.IsActive, cancellationToken);
        if (hub == null) return (false, "Hub di consegna non trovato, non attivo o non appartenente al cliente indicato.", null);

        if (!request.Items.Any()) return (false, "L'ordine deve contenere almeno un piatto.", null);

        // 2. Caricamento Piatti richiesti
        var plateIds = request.Items.Select(i => i.PlateId).ToList();
        var plates = await _context.Plates.Where(p => plateIds.Contains(p.Id)).ToListAsync(cancellationToken);
        
        if (plates.Count != plateIds.Distinct().Count()) 
            return (false, "Uno o più piatti richiesti non esistono a catalogo.", null);

        var now = DateTime.UtcNow;

        // 3. Elaborazione Righe, Prezzi e IVA
        var orderItems = new List<OrderItem>();
        int totalNetCents = 0;
        int totalVatCents = 0;
        int maxWorkingDaysRequired = 0;

        foreach (var reqItem in request.Items)
        {
            var plate = plates.First(p => p.Id == reqItem.PlateId);
            
            if (plate.WorkingDaysRequired > maxWorkingDaysRequired)
                maxWorkingDaysRequired = plate.WorkingDaysRequired;

            // --- DELEGA AL MOTORE DEI PREZZI ---
            int unitNetPrice = await _discountService.GetEffectivePriceAsync(request.CustomerId, plate.Id, cancellationToken);
            
            string? appliedNote = null;
            if (unitNetPrice != plate.BasePrice)
            {
                appliedNote = "Prezzo applicato da listino/sconti";
            }

            int lineNetCents = unitNetPrice * reqItem.Quantity;
            int lineVatCents = (int)Math.Round(lineNetCents * (plate.VatRate / 100m), MidpointRounding.AwayFromZero);

            totalNetCents += lineNetCents;
            totalVatCents += lineVatCents;

            orderItems.Add(new OrderItem
            {
                PlateId = plate.Id,
                PlateNameSnapshot = plate.Name,
                Quantity = reqItem.Quantity,
                UnitPriceNetCents = unitNetPrice,
                VatRate = plate.VatRate,
                AppliedDiscountNote = appliedNote
            });
        }

        // 4. Calcolo Data di Consegna Reale
        DateTime calculatedDelivery = request.BypassCalculator 
            ? request.RequestedDeliveryDate 
            : _workingDayCalculator.CalculateDeliveryDate(now, maxWorkingDaysRequired);

        // 5. Creazione Entità
        var order = new Order
        {
            CustomerId = customer.Id,
            DeliveryHubId = hub.Id, 
            OrderDate = now,
            CustomerReference = request.CustomerReference, 
            RequestedDeliveryDate = request.RequestedDeliveryDate,
            CalculatedDeliveryDate = calculatedDelivery,
            Status = OrderStatus.Pending,
            NetAmountCents = totalNetCents,
            VatAmountCents = totalVatCents,
            TotalGrossCents = totalNetCents + totalVatCents,
            ShippingCostCents = 0, 
            DeliveryNotes = request.DeliveryNotes,
            OrderItems = orderItems
        };

        _context.Orders.Add(order);
        
        await _context.SaveChangesAsync(cancellationToken); 

        var responseDto = await GetByIdAsync(order.Id, cancellationToken);
        return (true, "Ordine creato con successo.", responseDto);
    }

    public async Task<PaginatedResponseDto<OrderResponseDto>> GetPagedAsync(OrderQueryParameters filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.DeliveryHub) 
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.Trim().ToLower();
            query = query.Where(o => 
                o.OrderNumber.ToLower().Contains(searchTerm) || 
                (o.CustomerReference != null && o.CustomerReference.ToLower().Contains(searchTerm)) ||
                o.Customer!.BusinessName.ToLower().Contains(searchTerm));
        }

        if (filter.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == filter.CustomerId.Value);
            
        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.CalculatedDeliveryDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(o => o.CalculatedDeliveryDate <= filter.DateTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        
        bool isDesc = filter.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        
        IQueryable<Order> queryOrdered = filter.SortColumn?.ToLower() switch
        {
            "ordernumber"            => isDesc ? query.OrderByDescending(o => o.OrderSequence)          : query.OrderBy(o => o.OrderSequence),
            "orderdate"              => isDesc ? query.OrderByDescending(o => o.OrderDate)              : query.OrderBy(o => o.OrderDate),
            "customerbusinessname"   => isDesc ? query.OrderByDescending(o => o.Customer!.BusinessName) : query.OrderBy(o => o.Customer!.BusinessName),
            "deliveryhubname"        => isDesc ? query.OrderByDescending(o => o.DeliveryHub!.Name)      : query.OrderBy(o => o.DeliveryHub!.Name),
            "calculateddeliverydate" => isDesc ? query.OrderByDescending(o => o.CalculatedDeliveryDate) : query.OrderBy(o => o.CalculatedDeliveryDate),
            "requesteddeliverydate"  => isDesc ? query.OrderByDescending(o => o.RequestedDeliveryDate)  : query.OrderBy(o => o.RequestedDeliveryDate),
            "totalformatted"         => isDesc ? query.OrderByDescending(o => o.TotalGrossCents)        : query.OrderBy(o => o.TotalGrossCents),
            "statuslabel"            => isDesc ? query.OrderByDescending(o => o.Status)                 : query.OrderBy(o => o.Status),
            _                        => query.OrderBy(o => o.CalculatedDeliveryDate).ThenByDescending(o => o.OrderDate)
        };

        List<OrderResponseDto> items;

        if (filter.PageSize == -1)
        {
            items = await queryOrdered
                .Select(o => MapToDto(o))
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDto<OrderResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = 1,
                PageSize = totalCount > 0 ? totalCount : 1
            };
        }
        else
        {
            int validPage = filter.Page > 0 ? filter.Page : 1;
            int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

            items = await queryOrdered
                .Skip((validPage - 1) * validPageSize)
                .Take(validPageSize)
                .Select(o => MapToDto(o))
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDto<OrderResponseDto>
            {
                Items = items, TotalCount = totalCount, Page = validPage, PageSize = validPageSize
            };
        }
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.DeliveryHub) 
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order == null ? null : MapToDto(order);
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(Guid id, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order == null) return (false, "Ordine non trovato.");

        order.Status = newStatus;
        await _context.SaveChangesAsync(cancellationToken);

        return (true, $"Stato dell'ordine aggiornato a {newStatus}.");
    }
    
    private static OrderResponseDto MapToDto(Order o)
    {
        return new OrderResponseDto(
            o.Id, 
            o.CustomerId, 
            o.DeliveryHubId,
            o.Customer?.BusinessName ?? "N/D", 
            o.DeliveryHub?.Name ?? "Sede Non Specificata", 
            o.OrderNumber,          
            o.CustomerReference,    
            o.OrderDate, 
            o.RequestedDeliveryDate, 
            o.CalculatedDeliveryDate,
            o.Status, 
            o.NetAmountCents, 
            o.VatAmountCents, 
            o.TotalGrossCents, 
            o.ShippingCostCents,
            o.DeliveryNotes, 
            o.InvoiceNumber,
            o.OrderItems.Select(i => new OrderItemResponseDto(
                i.Id, i.PlateId, i.PlateNameSnapshot, i.Quantity, i.UnitPriceNetCents, i.VatRate, i.AppliedDiscountNote
            )).ToList()
        );
    }
}