using Roscoff.Core.Enums;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Dtos.Catalog;

// --- DTO PER LA CREAZIONE DELL'ORDINE (Da Angular a .NET) ---
public record CreateOrderRequestDto(
    Guid CustomerId,
    Guid DeliveryHubId,
    DateTime RequestedDeliveryDate, // La data scelta a schermo
    string? CustomerReference,      // <-- NUOVO: Riferimento ordine cliente (es. ODA 5487)
    string? DeliveryNotes,
    List<CreateOrderItemDto> Items,
    bool BypassCalculator = false   // <-- IL SEGRETO È QUI. Di default è false (per i clienti)
);
 
public record CreateOrderItemDto(
    int PlateId,
    int Quantity
);

// --- DTO PER LA LETTURA DELL'ORDINE (Da .NET ad Angular) ---
public record OrderResponseDto(
    Guid Id,
    Guid CustomerId,
    Guid DeliveryHubId, 
    string CustomerBusinessName,
    string DeliveryHubName, 
    string OrderNumber,             // <-- NUOVO: Codice generato (es. ORD-1001)
    string? CustomerReference,      // <-- NUOVO: Riferimento ordine cliente
    DateTime OrderDate,
    DateTime RequestedDeliveryDate,
    DateTime CalculatedDeliveryDate,
    OrderStatus Status,
    int NetAmountCents,
    int VatAmountCents,
    int TotalGrossCents,
    int ShippingCostCents,
    string? DeliveryNotes,
    string? InvoiceNumber,
    List<OrderItemResponseDto> Items
);

public record OrderItemResponseDto(
    int Id,
    int PlateId,
    string PlateNameSnapshot,
    int Quantity,
    int UnitPriceNetCents,
    decimal VatRate,
    string? AppliedDiscountNote
);

public class OrderQueryParameters : BasePaginationRequestDto
{
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    // Nota: Il campo Search è già ereditato da BasePaginationRequestDto
}

public record UpdateOrderStatusDto(
    OrderStatus Status
);