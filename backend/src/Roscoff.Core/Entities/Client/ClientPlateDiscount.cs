using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Core.Interfaces;

namespace Roscoff.Core.Entities.Client;

[Table("client_plate_discounts")]
[PrimaryKey(nameof(CustomerId), nameof(PlateId))]
public class ClientPlateDiscount : IAuditableEntity
{
    public Guid CustomerId { get; set; }
    public int PlateId { get; set; }

    // Il prezzo in centesimi, come da standard architetturale
    public int OverridePrice { get; set; } 

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    // --- Campi di Audit Manuali ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // --- Relazioni ---
    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(PlateId))]
    public Plate? Plate { get; set; }
}