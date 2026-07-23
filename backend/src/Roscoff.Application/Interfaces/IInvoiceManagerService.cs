using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Invoice;

namespace Roscoff.Application.Interfaces;

public interface IInvoiceManagerService
{
    // --- LETTURE (Ex QueryService) ---
    Task<PaginatedResponseDto<PendingCustomerSummaryDto>> GetPendingInvoicesSummaryAsync(
        PendingInvoiceQueryParameters filter,
        CancellationToken cancellationToken = default);

    Task<List<PendingCustomerOrderNodeDto>> GetPendingOrdersByCustomerAsync(
        Guid customerId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken = default);

    Task<PaginatedResponseDto<InvoiceHistorySummaryDto>> GetInvoicesHistoryAsync(
        InvoiceHistoryQueryParameters filter, 
        CancellationToken cancellationToken = default);

    // --- AZIONI (Ex ManagerService) ---
    Task<(bool Success, string Message)> DeleteInvoiceAsync(
        int ficDocumentId, 
        CancellationToken cancellationToken = default);
    
    // Nuovo metodo per recuperare il link pubblico della fattura
    Task<(bool Success, string? Url, string Message)> GetInvoiceUrlAsync(
        int ficDocumentId, 
        CancellationToken cancellationToken = default);
}