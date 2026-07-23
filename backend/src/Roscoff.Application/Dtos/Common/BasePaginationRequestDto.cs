namespace Roscoff.Application.Dtos.Common
{
    public abstract class BasePaginationRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? SortColumn { get; set; } = "CreatedAt";
        public string? SortDirection { get; set; } = "desc";
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class PaginatedResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
