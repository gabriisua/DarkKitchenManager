using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Dtos.Catalog;

public class MenuQueryParameters : BasePaginationRequestDto
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
