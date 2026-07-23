using System.ComponentModel.DataAnnotations;

namespace Roscoff.Application.Dtos.Catalog;

public record AllergenResponseDto(int Id, string Name, string Code, string? Description);

public class AllergenCreateDto
{
    [Required(ErrorMessage = "Il nome dell'allergene è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Il codice dell'allergene è obbligatorio.")]
    [StringLength(20, ErrorMessage = "Il codice non può superare i 20 caratteri.")]
    public string Code { get; set; } = null!;

    [StringLength(255, ErrorMessage = "La descrizione non può superare i 255 caratteri.")]
    public string? Description { get; set; }
}

public class AllergenUpdateDto
{
    [StringLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    public string? Name { get; set; }

    [StringLength(20, ErrorMessage = "Il codice non può superare i 20 caratteri.")]
    public string? Code { get; set; }

    [StringLength(255, ErrorMessage = "La descrizione non può superare i 255 caratteri.")]
    public string? Description { get; set; }
}