using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Roscoff.Core.Enums;
using Roscoff.Core.Interfaces;

namespace Roscoff.Core.Entities.Invoice;

[Table("orders")]
public class Order : BaseEntity<Guid>, IAuditableEntity
{
    [Required]
    public Guid CustomerId { get; set; }

    // --- NUOVO: RIFERIMENTO ALL'HUB DI CONSEGNA ---
    [Required]
    public Guid DeliveryHubId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    //campo sql di numerazione progressiva per le fatture
    public int OrderSequence { get; set; } 
    //Numero autogenerato della fattura/ddt/oda
    [StringLength(50)]
    public string OrderNumber { get; set; } = null!;
    //Codice modificabile dall'utente nonchè referenza d'ordine per clienti che inviano ordini d'acquisto 
    [StringLength(100)]
    public string? CustomerReference { get; set; }

    [Required]
    public DateTime RequestedDeliveryDate { get; set; }

    [Required]
    public DateTime CalculatedDeliveryDate { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    public int NetAmountCents { get; set; }
    
    [Required]
    public int VatAmountCents { get; set; }
    
    [Required]
    public int TotalGrossCents { get; set; }

    [Required]
    public int ShippingCostCents { get; set; }

    [StringLength(500)]
    public string? DeliveryNotes { get; set; }

    public int? FicInvoiceDocumentId { get; set; } 
    public string? InvoiceNumber { get; set; } 

    // --- Campi Audit ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // --- Relazioni ---
    [ForeignKey(nameof(CustomerId))]
    public Client.Customer? Customer { get; set; }

    // --- NUOVA RELAZIONE ---
    [ForeignKey(nameof(DeliveryHubId))]
    public Client.DeliveryHub? DeliveryHub { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}