using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Roscoff.Core.Interfaces;

namespace Roscoff.Core.Entities.Catalog;

[Table("menu_items")]
[PrimaryKey(nameof(MenuId), nameof(PlateId))] // Chiave primaria composta per EF Core
public class MenuItem : IAuditableEntity
{
    public int MenuId { get; set; }
    public int PlateId { get; set; }

    public int? OverridePrice { get; set; } 

    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public string? AvailableDaysOfWeek { get; set; } 

    // --- Campi di Audit Manuali ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // --- RELAZIONI ---
    [ForeignKey(nameof(MenuId))]
    public Menu? Menu { get; set; }

    [ForeignKey(nameof(PlateId))]
    public Plate? Plate { get; set; }
}