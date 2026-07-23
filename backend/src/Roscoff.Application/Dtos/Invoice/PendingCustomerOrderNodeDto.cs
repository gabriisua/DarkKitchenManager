namespace Roscoff.Application.Dtos.Invoice;

public record PendingCustomerOrderNodeDto(
    Guid OrderId,
    string OrderNumber,
    DateTime OrderDate,
    DateTime CalculatedDeliveryDate,
    int TotalGrossCents,
    string? CustomerReference,
    string? LatestErrorMessage 
);