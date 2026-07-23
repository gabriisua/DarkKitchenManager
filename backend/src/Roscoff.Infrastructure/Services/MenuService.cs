using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly RoscoffDbContext _context;

    public MenuService(RoscoffDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponseDto<MenuResponseDto>> GetAllAsync(MenuQueryParameters filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Menus
            .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.Plate)
                    .ThenInclude(p => p!.Category)
            .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.Plate)
                    .ThenInclude(p => p!.PlateIngredients) // ECCOLO QUI! Il nome corretto!
            .AsSplitQuery()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search;
            query = query.Where(m => m.Name.Contains(searchTerm) ||
                                     (m.Description != null && m.Description.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(m => m.Name == filter.Name);

        if (filter.IsActive.HasValue)
            query = query.Where(m => m.IsActive == filter.IsActive.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(m => m.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(m => m.CreatedAt <= filter.DateTo.Value);

        bool isDesc = filter.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = filter.SortColumn?.ToLower() switch
        {
            "name"     => isDesc ? query.OrderByDescending(m => m.Name)      : query.OrderBy(m => m.Name),
            "isactive" => isDesc ? query.OrderByDescending(m => m.IsActive)  : query.OrderBy(m => m.IsActive),
            _          => isDesc ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        int validPage = filter.Page > 0 ? filter.Page : 1;
        int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        // Tiriamo su i dati grezzi dal DB per evitare che EF Core esploda a fare la Somma in SQL
        var dbMenus = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync(cancellationToken);

        // Mappiamo i DTO in memoria
        var items = dbMenus.Select(m => new MenuResponseDto(
            m.Id,
            m.Name,
            m.Description,
            m.IsActive,
            m.CustomerId,
            m.CreatedAt,
            m.UpdatedAt,
            m.MenuItems.Select(mi => new MenuItemResponseDto(
                mi.MenuId,
                mi.PlateId,
                mi.Plate != null ? mi.Plate.Name : "N/D",
                mi.Plate != null && mi.Plate.PlateIngredients != null ? mi.Plate.PlateIngredients.Sum(pi => pi.WeightInGrams) : 0m, // Calcolo Peso corretto!
                mi.Plate != null ? mi.Plate.DaysToExpire : 3, // Shelf Life
                mi.Plate != null ? mi.Plate.BasePrice : 0,    
                mi.Plate != null && mi.Plate.Category != null ? mi.Plate.Category.Name : "Varie", 
                mi.OverridePrice,
                mi.AvailableFrom,
                mi.AvailableTo,
                mi.AvailableDaysOfWeek
            )).ToList()
        )).ToList();

        return new PaginatedResponseDto<MenuResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<MenuResponseDto?> GetByIdAsync(int id)
    {
        var menu = await _context.Menus
            .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.Plate)
                    .ThenInclude(p => p!.Category)
            .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.Plate)
                    .ThenInclude(p => p!.PlateIngredients) // Il nome corretto
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menu == null)
            return null;

        return new MenuResponseDto(
            menu.Id,
            menu.Name,
            menu.Description,
            menu.IsActive,
            menu.CustomerId,
            menu.CreatedAt,
            menu.UpdatedAt,
            menu.MenuItems.Select(mi => new MenuItemResponseDto(
                mi.MenuId,
                mi.PlateId,
                mi.Plate?.Name ?? "N/D",
                mi.Plate?.PlateIngredients?.Sum(pi => pi.WeightInGrams) ?? 0m, // Calcolo Peso
                mi.Plate?.DaysToExpire ?? 3,                                   // Shelf Life
                mi.Plate?.BasePrice ?? 0, 
                mi.Plate?.Category?.Name ?? "Varie", 
                mi.OverridePrice,
                mi.AvailableFrom,
                mi.AvailableTo,
                mi.AvailableDaysOfWeek
            )).ToList()
        );
    }

    public async Task<MenuResponseDto> CreateAsync(MenuCreateDto dto)
    {
        if (dto.CustomerId.HasValue)
        {
            var alreadyHasMenu = await _context.Menus
                .AnyAsync(m => m.CustomerId == dto.CustomerId.Value);
                
            if (alreadyHasMenu)
            {
                throw new InvalidOperationException("Il cliente ha già un menu dedicato attivo.");
            }
        }

        var menu = new Menu
        {
            Name = dto.Name,
            Description = dto.Description,
            CustomerId = dto.CustomerId,
            IsActive = true,
            MenuItems = dto.MenuItems.Select(mi => new MenuItem
            {
                PlateId = mi.PlateId,
                OverridePrice = mi.OverridePrice,
                AvailableFrom = mi.AvailableFrom,
                AvailableTo = mi.AvailableTo,
                AvailableDaysOfWeek = mi.AvailableDaysOfWeek
            }).ToList()
        };

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(menu.Id))!;
    }

    public async Task<MenuResponseDto?> UpdateAsync(int id, MenuUpdateDto dto)
    {
        var menu = await _context.Menus
            .Include(m => m.MenuItems)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menu == null)
            return null;

        if (dto.CustomerId.HasValue && dto.CustomerId != menu.CustomerId)
        {
            var alreadyHasMenu = await _context.Menus
                .AnyAsync(m => m.CustomerId == dto.CustomerId.Value && m.Id != id);
                
            if (alreadyHasMenu)
            {
                throw new InvalidOperationException("Il cliente ha già un altro menu dedicato attivo.");
            }
        }

        menu.Name = dto.Name ?? menu.Name;
        menu.Description = dto.Description ?? menu.Description;
        menu.IsActive = dto.IsActive ?? menu.IsActive;
        
        if (dto.CustomerId != Guid.Empty)
        {
            menu.CustomerId = dto.CustomerId;
        }

        if (dto.MenuItems != null)
        {
            var incomingPlateIds = dto.MenuItems.Select(mi => mi.PlateId).ToList();

            var itemsToRemove = menu.MenuItems
                .Where(mi => !incomingPlateIds.Contains(mi.PlateId))
                .ToList();

            foreach (var toRemove in itemsToRemove)
            {
                menu.MenuItems.Remove(toRemove);
            }

            foreach (var incomingItem in dto.MenuItems)
            {
                var existingItem = menu.MenuItems
                    .FirstOrDefault(mi => mi.PlateId == incomingItem.PlateId);

                if (existingItem != null)
                {
                    existingItem.OverridePrice = incomingItem.OverridePrice;
                    existingItem.AvailableFrom = incomingItem.AvailableFrom;
                    existingItem.AvailableTo = incomingItem.AvailableTo;
                    existingItem.AvailableDaysOfWeek = incomingItem.AvailableDaysOfWeek;
                }
                else
                {
                    menu.MenuItems.Add(new MenuItem
                    {
                        MenuId = menu.Id,
                        PlateId = incomingItem.PlateId,
                        OverridePrice = incomingItem.OverridePrice,
                        AvailableFrom = incomingItem.AvailableFrom,
                        AvailableTo = incomingItem.AvailableTo,
                        AvailableDaysOfWeek = incomingItem.AvailableDaysOfWeek
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(menu.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var menu = await _context.Menus.FindAsync(id);

        if (menu == null)
            return false;

        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync();

        return true;
    }
}