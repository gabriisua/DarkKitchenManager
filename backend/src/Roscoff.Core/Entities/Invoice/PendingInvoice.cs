using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Invoice;

public enum PendingInvoiceStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

[Table("pending_invoices")]
public class PendingInvoice : BaseEntity<int>
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public PendingInvoiceStatus Status { get; set; } = PendingInvoiceStatus.Pending;

    // Se la chiamata a FIC fallisce, salviamo qui il motivo (es. "P.IVA non valida")
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    // --- Relazioni ---
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
}