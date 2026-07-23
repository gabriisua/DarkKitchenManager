using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Roscoff.Core.Entities.Catalog;

namespace Roscoff.Core.Entities.Invoice;

[Table("order_items")]
public class OrderItem : BaseEntity<int>
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public int PlateId { get; set; }

    // SNAPSHOT: Salviamo il nome del piatto al momento dell'ordine per FIC
    [Required, StringLength(150)]
    public string PlateNameSnapshot { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    // Il prezzo NETTO finale concordato per unità (post Discount Engine) in centesimi
    [Required]
    public int UnitPriceNetCents { get; set; }

    // L'aliquota IVA applicata a questo piatto (es. 10 per il 10%)
    // Fondamentale per far ricalcolare a FIC i totali corretti
    [Required, Column(TypeName = "decimal(5,2)")]
    public decimal VatRate { get; set; }

    // (Opzionale ma utile) Tiene traccia se il prezzo deriva da una sovrascrittura o uno sconto
    [StringLength(50)]
    public string? AppliedDiscountNote { get; set; } 

    // --- Relazioni ---
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(PlateId))]
    public Plate? Plate { get; set; } // Lascialo nullable: se un giorno elimini un piatto, la riga d'ordine deve sopravvivere!
}
