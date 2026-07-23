using Roscoff.Core.Entities.Catalog;

namespace Roscoff.Application.Dtos.Catalog;

public record RequestPlateDto(
    string? Code,
    string Name,
    string? Description,
    int CategoryId,
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
    
    // --- AGGIUNTI QUI ---
    DietaryIconType DietaryIcon,
    bool IsWowPlate,
    bool IsXlPlate,

    List<PlateIngredientDto> Ingredients
);

public record UpdatePlateDto(
    string? Code,
    string? Name,
    string? Description,
    int CategoryId,
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
    
    // --- AGGIUNTI QUI ---
    DietaryIconType DietaryIcon,
    bool IsWowPlate,
    bool IsXlPlate,

    List<PlateIngredientDto>? Ingredients
);

public record PlateIngredientDto(
    int IngredientId,
    decimal WeightInGrams
);