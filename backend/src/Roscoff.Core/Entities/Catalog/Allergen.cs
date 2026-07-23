using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Catalog;

[Table("allergens")]
public class Allergen : BaseEntity<int>
{
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }
    
    [Required, StringLength(20)]
    public string Code { get; set; } = null!; 

    public ICollection<IngredientAllergen> IngredientAllergens { get; set; }
        = new List<IngredientAllergen>();
}