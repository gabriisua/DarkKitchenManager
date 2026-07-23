using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Dtos.Catalog;

public class AllergenQueryParameters : BasePaginationRequestDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}
