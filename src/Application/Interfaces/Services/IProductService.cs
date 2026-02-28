using Application.Common.Models;
using Application.Common.Results;
using Application.DTOs.Product;

namespace Application.Interfaces.Services;

public interface IProductService
{
    Task<Result<ProductDto>> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken);
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<PagedResult<ProductDto>>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
