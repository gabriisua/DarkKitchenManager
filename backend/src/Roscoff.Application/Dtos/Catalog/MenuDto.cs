namespace Roscoff.Application.Dtos.Catalog;

public record MenuResponseDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    Guid? CustomerId, // <--- Aggiunto per sapere a chi è assegnato
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<MenuItemResponseDto> MenuItems
);

public record MenuItemResponseDto(
    int MenuId,
    int PlateId,
    string PlateName,
    decimal PlateWeight,     // <-- NUOVO (Peso in grammi calcolato)
    int DaysToExpire,        // <-- NUOVO (Shelf Life per la data)
    int BasePrice,
    string CategoryName,
    int? OverridePrice,
    DateTime? AvailableFrom,
    DateTime? AvailableTo,
    string? AvailableDaysOfWeek
);

public record MenuCreateDto(
    string Name,
    string? Description,
    Guid? CustomerId, // <--- Aggiunto per l'assegnazione in fase di creazione
    List<MenuItemCreateDto> MenuItems
);

public record MenuItemCreateDto(
    int PlateId,
    int? OverridePrice,
    DateTime? AvailableFrom,
    DateTime? AvailableTo,
    string? AvailableDaysOfWeek
);

public record MenuUpdateDto(
    string? Name,
    string? Description,
    bool? IsActive,
    Guid? CustomerId, // <--- Aggiunto per modificare l'assegnazione
    List<MenuItemUpdateDto>? MenuItems
);

public record MenuItemUpdateDto(
    int PlateId,
    int? OverridePrice,
    DateTime? AvailableFrom,
    DateTime? AvailableTo,
    string? AvailableDaysOfWeek
);