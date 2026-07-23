using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Catalog;

[Table("categories")]
public class Category : BaseEntity<int>
{
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // Relazione
    public ICollection<Plate> Plates { get; set; } = new List<Plate>();
}