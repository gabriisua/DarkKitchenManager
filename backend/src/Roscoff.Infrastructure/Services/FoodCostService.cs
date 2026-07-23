using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class FoodCostService : IFoodCostService
{
    private readonly RoscoffDbContext _context;

    public FoodCostService(RoscoffDbContext context)
    {
        _context = context;
    }

    public async Task<int> CalculatePlateFoodCostAsync(int plateId)
    {
        var plate = await _context.Plates
            .Include(p => p.PlateIngredients)
            .ThenInclude(pi => pi.Ingredient)
            .FirstOrDefaultAsync(p => p.Id == plateId);

        if (plate == null || plate.PlateIngredients == null || !plate.PlateIngredients.Any())
        {
            return 0; 
        }

        // Usiamo decimal per mantenere la precisione assoluta
        decimal totalCost = 0m;

        foreach (var recipeItem in plate.PlateIngredients)
        {
            var ingredient = recipeItem.Ingredient;
            if (ingredient == null) continue;

            // Ora i tipi combaciano perfettamente con le tue entità
            decimal costPerKg = ingredient.CostPer1000g; 
            decimal weightInGrams = recipeItem.WeightInGrams; // <-- Corretto (una 'm')
            decimal yield = ingredient.YieldPercentage; 

            if (yield <= 0) yield = 100m; 

            // Aggiungiamo 'm' ai numeri per forzare il calcolo in formato decimal
            decimal itemEffectiveCost = (costPerKg * weightInGrams / 1000m) * (100m / yield);

            totalCost += itemEffectiveCost;
        }

        // Moltiplichiamo per 100m per convertire gli Euro (es. 2.26) in centesimi (es. 226)
        totalCost = totalCost * 100m;
        
        // Arrotonda sempre per eccesso al centesimo per tutelare i margini
        return (int)Math.Ceiling(totalCost);
    }
}