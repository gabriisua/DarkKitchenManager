using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Catalog;

[Table("ingredients")]
public class Ingredient : BaseEntity<int>
{
    [Required, StringLength(150)]
    public string Name { get; set; } = null!;
    
    [StringLength(1000)]
    public string? SubIngredients { get; set; }

    // --- Valori Nutrizionali Obbligatori (per 100g di prodotto) ---
    [Column(TypeName = "decimal(8, 2)")]
    public decimal EnergyKjPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal EnergyKcalPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal FatsPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal SaturatedFatsPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal CarbohydratesPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal SugarsPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal FibersPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal ProteinsPer100g { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal SaltPer100g { get; set; }
    
    // Costo per 1000g (mantenuto decimal per supportare centesimi o cifre precise)
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal CostPer1000g { get; set; }

    // --- Elementi per il Food Cost Reale ---
    [Required]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal YieldPercentage { get; set; } = 100.00m; // Es. 80.00 per l'80% di resa (20% scarto)

    // --------------------------------------------------------------

    public bool IsActive { get; set; } = true;

    public ICollection<IngredientAllergen> IngredientAllergens { get; set; }
        = new List<IngredientAllergen>();

    // Relazione con le Ricette (PlateIngredients)
    public ICollection<PlateIngredient> PlateIngredients { get; set; } = new List<PlateIngredient>();
}