using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class AllergenService : IAllergenService
{
    private readonly RoscoffDbContext _context;

    public AllergenService(RoscoffDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<AllergenResponseDto>> GetAllAllergensAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Allergens
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new AllergenResponseDto(a.Id, a.Name, a.Code, a.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResponseDto<AllergenResponseDto>> GetAllAsync(AllergenQueryParameters filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Allergens.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search;
            query = query.Where(a => a.Name.Contains(searchTerm) ||
                                     a.Code.Contains(searchTerm) ||
                                     (a.Description != null && a.Description.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(a => a.Name == filter.Name);

        if (!string.IsNullOrWhiteSpace(filter.Code))
            query = query.Where(a => a.Code == filter.Code);

        if (filter.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.DateTo.Value);

        bool isDesc = filter.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = filter.SortColumn?.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
            "code" => isDesc ? query.OrderByDescending(a => a.Code) : query.OrderBy(a => a.Code),
            _      => isDesc ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        int validPage = filter.Page > 0 ? filter.Page : 1;
        int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(a => new AllergenResponseDto(a.Id, a.Name, a.Code, a.Description))
            .ToListAsync(cancellationToken);

        return new PaginatedResponseDto<AllergenResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<AllergenResponseDto?> GetByIdAsync(int id)
    {
        var allergen = await _context.Allergens
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (allergen == null) return null;

        return new AllergenResponseDto(allergen.Id, allergen.Name, allergen.Code, allergen.Description);
    }

    public async Task<(bool Success, string Message, int? AllergenId)> CreateAsync(AllergenCreateDto request)
    {
        // Controllo duplicati su Codice o Nome
        if (await _context.Allergens.AnyAsync(a => a.Code == request.Code || a.Name == request.Name))
            return (false, "Esiste già un allergene con questo Nome o Codice.", null);

        var allergen = new Allergen
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description
        };

        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        return (true, "Allergene creato con successo.", allergen.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, AllergenUpdateDto request)
    {
        var allergen = await _context.Allergens.FirstOrDefaultAsync(a => a.Id == id);
        
        if (allergen == null) 
            return (false, "Allergene non trovato.");

        // Controllo se il nuovo codice o nome esiste già su UN ALTRO record
        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != allergen.Code)
        {
            if (await _context.Allergens.AnyAsync(a => a.Code == request.Code && a.Id != id))
                return (false, "Il codice specificato è già in uso da un altro allergene.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != allergen.Name)
        {
            if (await _context.Allergens.AnyAsync(a => a.Name == request.Name && a.Id != id))
                return (false, "Il nome specificato è già in uso da un altro allergene.");
        }

        allergen.Name = request.Name ?? allergen.Name;
        allergen.Code = request.Code ?? allergen.Code;
        allergen.Description = request.Description ?? allergen.Description;

        await _context.SaveChangesAsync();
        return (true, "Allergene aggiornato con successo.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var allergen = await _context.Allergens.FirstOrDefaultAsync(a => a.Id == id);
        
        if (allergen == null) 
            return (false, "Allergene non trovato.");

        // Hard Delete: La tua configurazione OnDelete(DeleteBehavior.Cascade) nel DbContext
        // eliminerà automaticamente le righe collegate in IngredientAllergens.
        _context.Allergens.Remove(allergen);
        await _context.SaveChangesAsync();
        
        return (true, "Allergene eliminato con successo.");
    }
}