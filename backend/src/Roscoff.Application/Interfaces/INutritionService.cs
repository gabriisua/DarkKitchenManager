using Roscoff.Application.Dtos.Catalog;

namespace Roscoff.Application.Interfaces;

public interface INutritionService
{
    Task<NutritionalSummaryDto> CalculatePlateNutritionAsync(int plateId);
}