using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Catalog;

[Table("menus")]
public class Menu : BaseEntity<int>
{
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // NUOVO: Relazione opzionale con il Cliente
    public Guid? CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Roscoff.Core.Entities.Client.Customer? Customer { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}