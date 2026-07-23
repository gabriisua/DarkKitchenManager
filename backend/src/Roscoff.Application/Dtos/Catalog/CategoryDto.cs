using System.ComponentModel.DataAnnotations;

namespace Roscoff.Application.Dtos.Catalog;

// 1. DTO per la LETTURA (Quello che il backend invia ad Angular)
public class CategoryReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// 2. DTO per la CREAZIONE (Quello che Angular invia al backend per le POST)
public class CategoryCreateDto
{
    [Required(ErrorMessage = "Il nome della categoria è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    public string Name { get; set; } = string.Empty;
}

// 3. DTO per la MODIFICA (Quello che Angular invia al backend per le PUT)
public class CategoryUpdateDto
{
    [Required(ErrorMessage = "Il nome della categoria è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}