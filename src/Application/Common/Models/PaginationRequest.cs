namespace Application.Common.Models;

public sealed class PaginationRequest
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int NormalizedPageNumber => PageNumber <= 0 ? 1 : PageNumber;
    public int NormalizedPageSize => PageSize <= 0 ? 10 : Math.Min(PageSize, MaxPageSize);
}
