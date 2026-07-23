namespace Roscoff.Application.Dtos.Catalog;

public record IngredientResponseDto(
    int Id,
    string Name,
    string? SubIngredients, // <-- NUOVO CAMPO
    decimal EnergyKjPer100g,
    decimal EnergyKcalPer100g,
    decimal FatsPer100g,
    decimal SaturatedFatsPer100g,
    decimal CarbohydratesPer100g,
    decimal SugarsPer100g,
    decimal FibersPer100g,
    decimal ProteinsPer100g,
    decimal SaltPer100g,
    decimal CostPer1000g,
    decimal YieldPercentage,
    List<int> AllergenIds
);

public record PlateIngredientResponseDto(
    int IngredientId,
    string IngredientName,
    string? SubIngredients, // <-- NUOVO CAMPO (Fondamentale per la composizione dell'etichetta)
    decimal WeightInGrams,
    decimal CostPer1000g 
);