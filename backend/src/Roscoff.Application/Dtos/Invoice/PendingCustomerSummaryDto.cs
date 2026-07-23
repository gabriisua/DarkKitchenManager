namespace Roscoff.Application.Dtos.Invoice;

public record PendingCustomerSummaryDto(
    Guid CustomerId,
    string BusinessName,
    string VatNumber,
    int OrdersCount,
    int NetAmountCents,
    int VatAmountCents,
    int TotalGrossCents,
    bool HasFailedInvoices 
);