using Application.DTOs.Product;

namespace WebAPI.ViewModels.Product;

public sealed class ProductListViewModel
{
    public required IReadOnlyCollection<ProductDto> Products { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
