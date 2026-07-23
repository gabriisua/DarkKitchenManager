using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Dtos.Invoice;

public class PendingInvoiceQueryParameters : BasePaginationRequestDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    // Eredita già Search per cercare il cliente per nome o P.IVA
}