namespace Roscoff.Application.Dtos.Invoice;

public record InvoiceHistorySummaryDto(
    int FicDocumentId,
    string InvoiceNumber,
    string CustomerName,
    int OrdersCount,
    int TotalGrossCents,
    DateTime MaxDeliveryDate
);