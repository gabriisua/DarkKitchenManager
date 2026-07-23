namespace Roscoff.Application.Dtos.Invoice;

public record FicDocumentResponseDto(
    int DocumentId,
    string InvoiceNumber,
    string? PdfUrl
);