using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common; // Assumo tu abbia qui il PaginatedResponseDto

namespace Roscoff.Application.Interfaces;

public interface ICategoryService
{
    // Ottiene tutte le categorie attive (per popolare le tendine)
    Task<IEnumerable<CategoryReadDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    
    // Ottiene la lista paginata (per le tabelle gestionali)
    Task<PaginatedResponseDto<CategoryReadDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    
    // Ottiene una singola categoria
    Task<CategoryReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    // Crea una nuova categoria
    Task<(bool Success, string Message, int? CategoryId)> CreateAsync(CategoryCreateDto request, CancellationToken cancellationToken = default);
    
    // Modifica una categoria esistente
    Task<(bool Success, string Message)> UpdateAsync(int id, CategoryUpdateDto request, CancellationToken cancellationToken = default);
    
    // Elimina una categoria (con controllo vincoli)
    Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}