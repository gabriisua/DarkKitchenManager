using Roscoff.Application.Dtos.Common;
using Roscoff.Core.Entities.Catalog;

namespace Roscoff.Application.Dtos.Catalog;

public class PlateQueryParameters : BasePaginationRequestDto
{
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public PlateLineType? LineType { get; set; }
    public DietaryIconType? DietaryIcon { get; set; } 
}