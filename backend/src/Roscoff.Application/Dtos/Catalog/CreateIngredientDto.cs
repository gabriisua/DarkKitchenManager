namespace Roscoff.Application.Dtos.Catalog;

public class CreateIngredientDto
{
    public string Name { get; set; } = null!;

    // --- NUOVO CAMPO ---
    public string? SubIngredients { get; set; }

    // Valori nutrizionali
    public decimal EnergyKjPer100g { get; set; }

    public decimal EnergyKcalPer100g { get; set; }

    public decimal FatsPer100g { get; set; }

    public decimal SaturatedFatsPer100g { get; set; }

    public decimal CarbohydratesPer100g { get; set; }

    public decimal SugarsPer100g { get; set; }

    public decimal FibersPer100g { get; set; }

    public decimal ProteinsPer100g { get; set; }

    public decimal SaltPer100g { get; set; }

    // Costi
    public decimal CostPer1000g { get; set; }

    // Resa
    public decimal YieldPercentage { get; set; }

    // Allergeni
    public List<int>? AllergenIds { get; set; }
}