using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Interfaces;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class NutritionService : INutritionService
{
    private readonly RoscoffDbContext _context;

    public NutritionService(RoscoffDbContext context)
    {
        _context = context;
    }

    public async Task<NutritionalSummaryDto> CalculatePlateNutritionAsync(int plateId)
    {
        // Includiamo a cascata: Piatto -> Ingredienti del Piatto -> Ingrediente -> Allergeni dell'Ingrediente -> Anagrafica Allergene
        var plate = await _context.Plates
            .Include(p => p.PlateIngredients)
                .ThenInclude(pi => pi.Ingredient)
                    .ThenInclude(i => i.IngredientAllergens)
                        .ThenInclude(ia => ia.Allergen)
            .FirstOrDefaultAsync(p => p.Id == plateId);

        var summary = new NutritionalSummaryDto();

        if (plate == null || plate.PlateIngredients == null || !plate.PlateIngredients.Any())
        {
            return summary; 
        }

        // Dictionary per collezionare gli allergeni evitando duplicati (chiave: Id dell'allergene)
        var uniqueAllergens = new Dictionary<int, AllergenResponseDto>();

        foreach (var recipeItem in plate.PlateIngredients)
        {
            var ingredient = recipeItem.Ingredient;
            if (ingredient == null) continue;

            decimal weight = recipeItem.WeightInGrams;
            summary.TotalWeightGrams += weight;
            
            // Moltiplicatore per riproporzionare i valori (i dati nel DB sono per 100g, quindi peso / 100)
            decimal multiplier = weight / 100m;

            // Aggregazione macro e micro nutrienti
            summary.EnergyKcal += ingredient.EnergyKcalPer100g * multiplier;
            summary.Fats += ingredient.FatsPer100g * multiplier;
            summary.SaturatedFats += ingredient.SaturatedFatsPer100g * multiplier;
            summary.Carbohydrates += ingredient.CarbohydratesPer100g * multiplier;
            summary.Sugars += ingredient.SugarsPer100g * multiplier;
            summary.Fibers += ingredient.FibersPer100g * multiplier;
            summary.Proteins += ingredient.ProteinsPer100g * multiplier;
            summary.Salt += ingredient.SaltPer100g * multiplier;

            // Estrazione degli allergeni dell'ingrediente corrente
            if (ingredient.IngredientAllergens != null)
            {
                foreach (var ia in ingredient.IngredientAllergens)
                {
                    if (ia.Allergen != null && !uniqueAllergens.ContainsKey(ia.Allergen.Id))
                    {
                        uniqueAllergens.Add(
                            ia.Allergen.Id, 
                            new AllergenResponseDto(ia.Allergen.Id, ia.Allergen.Name, ia.Allergen.Description, ia.Allergen.Code)
                        );
                    }
                }
            }
        }

        // Arrotondamento matematico a 2 decimali per pulizia della risposta JSON
        summary.EnergyKcal = Math.Round(summary.EnergyKcal, 2);
        summary.Fats = Math.Round(summary.Fats, 2);
        summary.SaturatedFats = Math.Round(summary.SaturatedFats, 2);
        summary.Carbohydrates = Math.Round(summary.Carbohydrates, 2);
        summary.Sugars = Math.Round(summary.Sugars, 2);
        summary.Fibers = Math.Round(summary.Fibers, 2);
        summary.Proteins = Math.Round(summary.Proteins, 2);
        summary.Salt = Math.Round(summary.Salt, 2);

        // Mappatura degli allergeni univoci raccolti
        summary.Allergens = uniqueAllergens.Values.ToList();

        return summary;
    }
}