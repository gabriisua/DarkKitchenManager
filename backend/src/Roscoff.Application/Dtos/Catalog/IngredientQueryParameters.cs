using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Dtos.Catalog;

public class IngredientQueryParameters : BasePaginationRequestDto
{
    public string? Name { get; set; }
    public decimal? MinEnergyKcal { get; set; }
    public decimal? MaxEnergyKcal { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
    public bool? IsActive { get; set; }
}
