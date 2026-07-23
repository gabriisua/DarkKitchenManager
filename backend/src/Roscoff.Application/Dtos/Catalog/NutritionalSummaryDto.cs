namespace Roscoff.Application.Dtos.Catalog; // O il namespace che preferisci

public class NutritionalSummaryDto
{
    public decimal TotalWeightGrams { get; set; }
    public decimal EnergyKcal { get; set; }
    public decimal Fats { get; set; }
    public decimal SaturatedFats { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Sugars { get; set; }
    public decimal Fibers { get; set; }
    public decimal Proteins { get; set; }
    public decimal Salt { get; set; }
    
    // Aggiungiamo la lista degli allergeni riciclando il tuo DTO!
    public List<AllergenResponseDto> Allergens { get; set; } = new();
}