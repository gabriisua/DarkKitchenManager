using System.Collections.Generic;
using Roscoff.Core.Entities.Catalog;

namespace Roscoff.Application.Dtos.Catalog;

public record PlateResponseDto(
    int Id,
    string? Code,
    string Name,
    string? Description,
    string CategoryName,
    int BasePrice,       
    int PackagingCost,
    decimal VatRate,
    string? EanCode,
    int? MicrowaveWattage,
    decimal? MicrowaveMinutes,
    string? PreparationInstructions,
    int DaysToExpire, 
    string? ProductType,
    string? PackagingDescription,
    string? StorageConditions,
    string? PreservationTechnology,
    PlateLineType LineType, 
    DietaryIconType DietaryIcon,
    List<PlateIngredientResponseDto> Ingredients
);