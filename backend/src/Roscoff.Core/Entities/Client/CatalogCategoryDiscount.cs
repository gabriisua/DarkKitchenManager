using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Core.Interfaces;

namespace Roscoff.Core.Entities.Client;

[Table("client_category_discounts")]
[PrimaryKey(nameof(CustomerId), nameof(CategoryId))]
public class ClientCategoryDiscount : IAuditableEntity
{
    public Guid CustomerId { get; set; }
    public int CategoryId { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }

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

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }
}