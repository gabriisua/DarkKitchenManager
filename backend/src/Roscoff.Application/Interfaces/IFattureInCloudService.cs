using Roscoff.Application.Dtos.Invoice;
using Roscoff.Core.Entities.Invoice;

namespace Roscoff.Application.Interfaces;

public interface IFattureInCloudService
{
    // Accetta una lista di ordini (dello stesso cliente) da accorpare
    Task<FicDocumentResponseDto> CreateInvoiceForCustomerAsync(List<Order> orders, CancellationToken cancellationToken = default);
    
    Task DeleteInvoiceAsync(int documentId, CancellationToken cancellationToken = default);
    
    // Nuovo metodo che restituisce la stringa con l'URL pubblico della fattura
    Task<string> GetInvoiceUrlAsync(int documentId, CancellationToken cancellationToken = default);
}