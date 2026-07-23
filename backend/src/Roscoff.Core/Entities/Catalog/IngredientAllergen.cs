using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Catalog;

[Table("ingredient_allergens")]
public class IngredientAllergen
{
    public int IngredientId { get; set; }

    public Ingredient Ingredient { get; set; } = null!;

    public int AllergenId { get; set; }

    public Allergen Allergen { get; set; } = null!;
}