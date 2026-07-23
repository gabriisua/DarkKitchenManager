using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class IngredientService : IIngredientService
{
    private readonly RoscoffDbContext _context;

    public IngredientService(RoscoffDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponseDto<IngredientResponseDto>> GetAllAsync(IngredientQueryParameters filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Ingredients
            .Include(i => i.IngredientAllergens)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search;
            query = query.Where(i => i.Name.Contains(searchTerm) || 
                                     (i.SubIngredients != null && i.SubIngredients.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(i => i.Name == filter.Name);

        if (filter.MinEnergyKcal.HasValue)
            query = query.Where(i => i.EnergyKcalPer100g >= filter.MinEnergyKcal.Value);

        if (filter.MaxEnergyKcal.HasValue)
            query = query.Where(i => i.EnergyKcalPer100g <= filter.MaxEnergyKcal.Value);

        if (filter.MinCost.HasValue)
            query = query.Where(i => i.CostPer1000g >= filter.MinCost.Value);

        if (filter.MaxCost.HasValue)
            query = query.Where(i => i.CostPer1000g <= filter.MaxCost.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(i => i.IsActive == filter.IsActive.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(i => i.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(i => i.CreatedAt <= filter.DateTo.Value);

        bool isDesc = filter.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = filter.SortColumn?.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
            "energykcal" => isDesc
                ? query.OrderByDescending(i => i.EnergyKcalPer100g)
                : query.OrderBy(i => i.EnergyKcalPer100g),
            "cost" => isDesc ? query.OrderByDescending(i => i.CostPer1000g) : query.OrderBy(i => i.CostPer1000g),
            "yield" => isDesc ? query.OrderByDescending(i => i.YieldPercentage) : query.OrderBy(i => i.YieldPercentage),
            _ => isDesc ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        int validPage = filter.Page > 0 ? filter.Page : 1;
        int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(i => new IngredientResponseDto(
                i.Id,
                i.Name,
                i.SubIngredients, // --- NUOVO CAMPO AGGIUNTO ---
                i.EnergyKjPer100g,
                i.EnergyKcalPer100g,
                i.FatsPer100g,
                i.SaturatedFatsPer100g,
                i.CarbohydratesPer100g,
                i.SugarsPer100g,
                i.FibersPer100g,
                i.ProteinsPer100g,
                i.SaltPer100g,
                i.CostPer1000g,
                i.YieldPercentage,
                i.IngredientAllergens.Select(ia => ia.AllergenId).ToList()
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResponseDto<IngredientResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<IngredientResponseDto> CreateIngredientAsync(CreateIngredientDto dto)
    {
        var allergenIds = dto.AllergenIds ?? new List<int>();

        var allergens = await _context.Allergens
            .Where(a => allergenIds.Contains(a.Id))
            .ToListAsync();

        var ingredient = new Ingredient
        {
            Name = dto.Name,
            SubIngredients = dto.SubIngredients, // --- NUOVO CAMPO AGGIUNTO ---
            EnergyKjPer100g = dto.EnergyKjPer100g,
            EnergyKcalPer100g = dto.EnergyKcalPer100g,
            FatsPer100g = dto.FatsPer100g,
            SaturatedFatsPer100g = dto.SaturatedFatsPer100g,
            CarbohydratesPer100g = dto.CarbohydratesPer100g,
            SugarsPer100g = dto.SugarsPer100g,
            FibersPer100g = dto.FibersPer100g,
            ProteinsPer100g = dto.ProteinsPer100g,
            SaltPer100g = dto.SaltPer100g,
            CostPer1000g = dto.CostPer1000g,
            YieldPercentage = dto.YieldPercentage,
            IsActive = true
        };

        ingredient.IngredientAllergens = allergens
            .Select(a => new IngredientAllergen
            {
                AllergenId = a.Id,
                Ingredient = ingredient
            })
            .ToList();

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        return MapToResponseDto(ingredient);
    }

    public async Task<IngredientResponseDto?> GetIngredientByIdAsync(int id)
    {
        var ingredient = await _context.Ingredients
            .Include(i => i.IngredientAllergens)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (ingredient == null) return null;

        return MapToResponseDto(ingredient);
    }

    public async Task<IEnumerable<IngredientResponseDto>> GetAllIngredientsAsync()
    {
        var ingredients = await _context.Ingredients
            .Include(i => i.IngredientAllergens)
            .AsNoTracking()
            .ToListAsync();

        return ingredients.Select(MapToResponseDto);
    }

    private static IngredientResponseDto MapToResponseDto(Ingredient entity)
    {
        var allergenIds = entity.IngredientAllergens?
            .Select(ia => ia.AllergenId)
            .ToList() ?? new List<int>();

        return new IngredientResponseDto(
            entity.Id,
            entity.Name,
            entity.SubIngredients, // --- NUOVO CAMPO AGGIUNTO ---
            entity.EnergyKjPer100g,
            entity.EnergyKcalPer100g,
            entity.FatsPer100g,
            entity.SaturatedFatsPer100g,
            entity.CarbohydratesPer100g,
            entity.SugarsPer100g,
            entity.FibersPer100g,
            entity.ProteinsPer100g,
            entity.SaltPer100g,
            entity.CostPer1000g,
            entity.YieldPercentage,
            allergenIds
        );
    }

    public async Task<IngredientResponseDto?> UpdateIngredientAsync(int id, UpdateIngredientDto dto)
    {
        var ingredient = await _context.Ingredients
            .Include(i => i.IngredientAllergens)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (ingredient == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            ingredient.Name = dto.Name;

        ingredient.SubIngredients = dto.SubIngredients; // --- NUOVO CAMPO AGGIUNTO ---

        ingredient.EnergyKjPer100g = dto.EnergyKjPer100g ?? ingredient.EnergyKjPer100g;
        ingredient.EnergyKcalPer100g = dto.EnergyKcalPer100g ?? ingredient.EnergyKcalPer100g;
        ingredient.FatsPer100g = dto.FatsPer100g ?? ingredient.FatsPer100g;
        ingredient.SaturatedFatsPer100g = dto.SaturatedFatsPer100g ?? ingredient.SaturatedFatsPer100g;
        ingredient.CarbohydratesPer100g = dto.CarbohydratesPer100g ?? ingredient.CarbohydratesPer100g;
        ingredient.SugarsPer100g = dto.SugarsPer100g ?? ingredient.SugarsPer100g;
        ingredient.FibersPer100g = dto.FibersPer100g ?? ingredient.FibersPer100g;
        ingredient.ProteinsPer100g = dto.ProteinsPer100g ?? ingredient.ProteinsPer100g;
        ingredient.SaltPer100g = dto.SaltPer100g ?? ingredient.SaltPer100g;
        ingredient.CostPer1000g = dto.CostPer1000g;
        ingredient.YieldPercentage = dto.YieldPercentage;

        var newAllergenIds = dto.AllergenIds ?? new List<int>();
        var currentAllergenIds = ingredient.IngredientAllergens.Select(ia => ia.AllergenId).ToList();

        var toRemove = ingredient.IngredientAllergens
            .Where(ia => !newAllergenIds.Contains(ia.AllergenId))
            .ToList();

        foreach (var item in toRemove)
        {
            ingredient.IngredientAllergens.Remove(item);
        }

        var toAddIds = newAllergenIds.Except(currentAllergenIds).ToList();

        if (toAddIds.Any())
        {
            var allergensToAdd = await _context.Allergens
                .Where(a => toAddIds.Contains(a.Id))
                .ToListAsync();

            foreach (var allergen in allergensToAdd)
            {
                ingredient.IngredientAllergens.Add(new IngredientAllergen
                {
                    AllergenId = allergen.Id,
                    Allergen = allergen,
                    Ingredient = ingredient
                });
            }
        }

        await _context.SaveChangesAsync();

        return MapToResponseDto(ingredient);
    }

    public async Task<bool> SoftDeleteIngredientAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);

        if (ingredient == null)
            return false;

        ingredient.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }
}