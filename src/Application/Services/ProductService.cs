using Application.Common.Models;
using Application.Common.Results;
using Application.DTOs.Product;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Repositories;
using FluentValidation;

namespace Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductRequestDto> _createValidator;
    private readonly IValidator<UpdateProductRequestDto> _updateValidator;

    public ProductService(
        IProductRepository productRepository,
        IValidator<CreateProductRequestDto> createValidator,
        IValidator<UpdateProductRequestDto> updateValidator)
    {
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        await _productRepository.AddAsync(product, cancellationToken);

        return Result<ProductDto>.Success(Map(product));
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found.");
        }

        return Result<ProductDto>.Success(Map(product));
    }

    public async Task<Result<PagedResult<ProductDto>>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = request.NormalizedPageNumber;
        var pageSize = request.NormalizedPageSize;

        var totalCount = await _productRepository.CountAsync(cancellationToken);
        var items = await _productRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);

        return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>
        {
            Items = items.Select(Map).ToArray(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var existing = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return Result<ProductDto>.Failure("Product not found.");
        }

        existing.Name = request.Name.Trim();
        existing.Description = request.Description.Trim();
        existing.Price = request.Price;
        existing.StockQuantity = request.StockQuantity;
        existing.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(existing, cancellationToken);

        return Result<ProductDto>.Success(Map(existing));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return Result.Failure("Product not found.");
        }

        await _productRepository.DeleteAsync(existing, cancellationToken);
        return Result.Success();
    }

    private static ProductDto Map(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        StockQuantity = product.StockQuantity
    };
}
