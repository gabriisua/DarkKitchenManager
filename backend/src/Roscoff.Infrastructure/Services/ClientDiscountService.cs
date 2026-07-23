using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Client; 
using Roscoff.Application.Dtos.Common; 
using Roscoff.Application.Interfaces;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class ClientDiscountService : IClientDiscountService
{
    private readonly RoscoffDbContext _context;

    public ClientDiscountService(RoscoffDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // --- 1. CORE: MOTORE SCONTI ---
    // ==========================================
    public async Task<int> GetEffectivePriceAsync(Guid customerId, int plateId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // --- PRIORITÀ 1: MENU DEDICATO (Listino Esclusivo) ---
        var menuOverride = await _context.Menus
            .AsNoTracking()
            .Where(m => m.CustomerId == customerId && m.IsActive)
            .SelectMany(m => m.MenuItems)
            .Where(mi => mi.PlateId == plateId)
            .Where(mi => (mi.AvailableFrom == null || mi.AvailableFrom <= now) &&
                         (mi.AvailableTo == null || mi.AvailableTo >= now))
            .FirstOrDefaultAsync(cancellationToken);

        if (menuOverride?.OverridePrice != null)
            return menuOverride.OverridePrice.Value;

        // --- PRIORITÀ 2: SCONTO PIATTO (Override Puntuale) ---
        var plateDiscount = await _context.ClientPlateDiscounts
            .AsNoTracking()
            .Where(d => d.CustomerId == customerId && d.PlateId == plateId)
            .Where(d => (d.ValidFrom == null || d.ValidFrom <= now) && 
                        (d.ValidTo == null || d.ValidTo >= now))
            .FirstOrDefaultAsync(cancellationToken);

        if (plateDiscount != null)
            return plateDiscount.OverridePrice;

        // Recuperiamo il piatto per le prossime priorità
        var plate = await _context.Plates
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == plateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Piatto con ID {plateId} non trovato.");

        // --- PRIORITÀ 3: SCONTO CATEGORIA ---
        var categoryDiscount = await _context.ClientCategoryDiscounts
            .AsNoTracking()
            .Where(d => d.CustomerId == customerId && d.CategoryId == plate.CategoryId)
            .Where(d => (d.ValidFrom == null || d.ValidFrom <= now) && 
                        (d.ValidTo == null || d.ValidTo >= now))
            .FirstOrDefaultAsync(cancellationToken);

        if (categoryDiscount != null)
        {
            decimal multiplier = (100m - categoryDiscount.DiscountPercentage) / 100m;
            decimal discountedPrice = plate.BasePrice * multiplier;
            return (int)Math.Ceiling(discountedPrice);
        }

        // --- PRIORITÀ 4: PREZZO BASE ---
        return plate.BasePrice;
    }

    // ==========================================
    // --- 2. GET PAGINATE GESTIONALI ---
    // ==========================================
    public async Task<PaginatedResponseDto<PlateDiscountDto>> GetPagedPlateDiscountsAsync(DiscountQueryParameters parameters)
    {
        var query = _context.ClientPlateDiscounts
            .Include(d => d.Customer)
            .Include(d => d.Plate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search;
            query = query.Where(d => 
                (d.Customer!.BusinessName != null && d.Customer.BusinessName.Contains(search)) ||
                (d.Customer!.Email != null && d.Customer.Email.Contains(search)) ||
                (d.Plate!.Name != null && d.Plate.Name.Contains(search))
            );
        }

        var now = DateTime.UtcNow;
        if (parameters.IsActive.HasValue)
        {
            if (parameters.IsActive.Value)
                query = query.Where(d => (d.ValidFrom == null || d.ValidFrom <= now) && (d.ValidTo == null || d.ValidTo >= now));
            else
                query = query.Where(d => d.ValidTo != null && d.ValidTo < now);
        }

        if (parameters.DateFrom.HasValue)
            query = query.Where(d => d.CreatedAt >= parameters.DateFrom.Value);

        if (parameters.DateTo.HasValue)
            query = query.Where(d => d.CreatedAt <= parameters.DateTo.Value);

        bool isDesc = string.Equals(parameters.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = parameters.SortColumn?.ToLower() switch
        {
            "customer" => isDesc ? query.OrderByDescending(d => d.Customer!.BusinessName ?? d.Customer.Email) : query.OrderBy(d => d.Customer!.BusinessName ?? d.Customer.Email),
            "plate"    => isDesc ? query.OrderByDescending(d => d.Plate!.Name) : query.OrderBy(d => d.Plate!.Name),
            "price"    => isDesc ? query.OrderByDescending(d => d.OverridePrice) : query.OrderBy(d => d.OverridePrice),
            _          => isDesc ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        int validPage = parameters.Page > 0 ? parameters.Page : 1;
        int validPageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(d => new PlateDiscountDto
            {
                CustomerId = d.CustomerId,
                BusinessName = d.Customer!.BusinessName ?? d.Customer.Email,
                PlateId = d.PlateId,
                PlateName = d.Plate!.Name, 
                OverridePrice = d.OverridePrice,
                ValidFrom = d.ValidFrom,
                ValidTo = d.ValidTo
            })
            .AsNoTracking()
            .ToListAsync();

        return new PaginatedResponseDto<PlateDiscountDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<PaginatedResponseDto<CategoryDiscountDto>> GetPagedCategoryDiscountsAsync(DiscountQueryParameters parameters)
    {
        var query = _context.ClientCategoryDiscounts
            .Include(d => d.Customer)
            .Include(d => d.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search;
            query = query.Where(d => 
                (d.Customer!.BusinessName != null && d.Customer.BusinessName.Contains(search)) ||
                (d.Customer!.Email != null && d.Customer.Email.Contains(search)) ||
                (d.Category!.Name != null && d.Category.Name.Contains(search))
            );
        }

        var now = DateTime.UtcNow;
        if (parameters.IsActive.HasValue)
        {
            if (parameters.IsActive.Value)
                query = query.Where(d => (d.ValidFrom == null || d.ValidFrom <= now) && (d.ValidTo == null || d.ValidTo >= now));
            else
                query = query.Where(d => d.ValidTo != null && d.ValidTo < now);
        }

        if (parameters.DateFrom.HasValue)
            query = query.Where(d => d.CreatedAt >= parameters.DateFrom.Value);

        if (parameters.DateTo.HasValue)
            query = query.Where(d => d.CreatedAt <= parameters.DateTo.Value);

        bool isDesc = string.Equals(parameters.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = parameters.SortColumn?.ToLower() switch
        {
            "customer" => isDesc ? query.OrderByDescending(d => d.Customer!.BusinessName ?? d.Customer.Email) : query.OrderBy(d => d.Customer!.BusinessName ?? d.Customer.Email),
            "category" => isDesc ? query.OrderByDescending(d => d.Category!.Name) : query.OrderBy(d => d.Category!.Name),
            "discount" => isDesc ? query.OrderByDescending(d => d.DiscountPercentage) : query.OrderBy(d => d.DiscountPercentage),
            _          => isDesc ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        int validPage = parameters.Page > 0 ? parameters.Page : 1;
        int validPageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(d => new CategoryDiscountDto
            {
                CustomerId = d.CustomerId,
                BusinessName = d.Customer!.BusinessName ?? d.Customer.Email,
                CategoryId = d.CategoryId,
                CategoryName = d.Category!.Name,
                DiscountPercentage = d.DiscountPercentage,
                ValidFrom = d.ValidFrom,
                ValidTo = d.ValidTo
            })
            .AsNoTracking()
            .ToListAsync();

        return new PaginatedResponseDto<CategoryDiscountDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    // ==========================================
    // --- 3. HARD DELETE ---
    // ==========================================
    public async Task<(bool Success, string Message)> DeletePlateDiscountAsync(Guid customerId, int plateId)
    {
        var existing = await _context.ClientPlateDiscounts
            .FirstOrDefaultAsync(d => d.CustomerId == customerId && d.PlateId == plateId);

        if (existing == null) return (false, "Sconto piatto non trovato.");

        _context.ClientPlateDiscounts.Remove(existing);
        await _context.SaveChangesAsync();
        
        return (true, "Sconto piatto eliminato definitivamente.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoryDiscountAsync(Guid customerId, int categoryId)
    {
        var existing = await _context.ClientCategoryDiscounts
            .FirstOrDefaultAsync(d => d.CustomerId == customerId && d.CategoryId == categoryId);

        if (existing == null) return (false, "Sconto categoria non trovato.");

        _context.ClientCategoryDiscounts.Remove(existing);
        await _context.SaveChangesAsync();
        
        return (true, "Sconto categoria eliminato definitivamente.");
    }
}