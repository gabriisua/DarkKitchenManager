namespace Roscoff.Application.Dtos.Invoice;

public class InvoiceHistoryQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Filtro testo libero (es. Cerca "FATT-2026", "Mario Rossi" o P.IVA)
    public string? Search { get; set; }
    
    // Ordinamento (es. "date", "customer", "invoice", "total")
    public string? SortColumn { get; set; } 
    
    // "asc" o "desc"
    public string? SortDirection { get; set; } = "desc"; 
    
    // Filtro per data di consegna/fatturazione
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}