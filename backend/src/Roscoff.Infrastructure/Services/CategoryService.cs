using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Infrastructure.Data; // Il namespace del tuo DbContext

namespace Roscoff.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly RoscoffDbContext _context;

    public CategoryService(RoscoffDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryReadDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryReadDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResponseDto<CategoryReadDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        int validPage = page > 0 ? page : 1;
        int validPageSize = pageSize > 0 ? pageSize : 10;

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(c => new CategoryReadDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PaginatedResponseDto<CategoryReadDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<CategoryReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null) return null;

        return new CategoryReadDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }

    public async Task<(bool Success, string Message, int? CategoryId)> CreateAsync(CategoryCreateDto request, CancellationToken cancellationToken = default)
    {
        // Controllo se esiste già una categoria con lo stesso nome
        if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == request.Name.ToLower(), cancellationToken))
            return (false, "Esiste già una categoria con questo nome.", null);

        var category = new Category
        {
            Name = request.Name,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Categoria creata con successo.", category.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, CategoryUpdateDto request, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category == null) return (false, "Categoria non trovata.");

        // Controllo omonimie escludendo la categoria che stiamo modificando
        if (await _context.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == request.Name.ToLower(), cancellationToken))
            return (false, "Esiste già un'altra categoria con questo nome.");

        category.Name = request.Name;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        
        return (true, "Categoria aggiornata con successo.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.Plates) // Includo per controllare le foreign key
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        
        if (category == null) return (false, "Categoria non trovata.");

        // Blocco l'eliminazione se ci sono piatti agganciati
        if (category.Plates.Any())
            return (false, "Impossibile eliminare la categoria: ci sono piatti associati. Disattivala o sposta i piatti prima di procedere.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        
        return (true, "Categoria eliminata con successo.");
    }
}