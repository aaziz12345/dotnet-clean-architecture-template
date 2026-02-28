using Application.Common.Models;
using Application.DTOs.Product;
using Application.Services;
using Domain.Entities;
using Domain.Repositories;
using FluentValidation;
using Moq;

namespace UnitTests;

public sealed class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly IValidator<CreateProductRequestDto> _createValidator = new Application.Validators.CreateProductRequestValidator();
    private readonly IValidator<UpdateProductRequestDto> _updateValidator = new Application.Validators.UpdateProductRequestValidator();

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var service = CreateService();
        var request = new CreateProductRequestDto
        {
            Name = "Keyboard",
            Description = "Gaming keyboard",
            Price = 99.99m,
            StockQuantity = 10
        };

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(request.Name, result.Value!.Name);
        _productRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenNameIsMissing()
    {
        var service = CreateService();
        var request = new CreateProductRequestDto
        {
            Name = string.Empty,
            Description = "Gaming keyboard",
            Price = 99.99m,
            StockQuantity = 10
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenNotFound()
    {
        var service = CreateService();
        var id = Guid.NewGuid();

        _productRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var result = await service.GetByIdAsync(id, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenProductExists()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var existing = new Product { Id = id, Name = "Old", Description = "Old", Price = 10, StockQuantity = 2 };
        var request = new UpdateProductRequestDto { Name = "New", Description = "New", Price = 20, StockQuantity = 5 };

        _productRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var result = await service.UpdateAsync(id, request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", result.Value!.Name);
        _productRepository.Verify(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        var service = CreateService();
        var id = Guid.NewGuid();

        _productRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var result = await service.DeleteAsync(id, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product not found.", result.Error);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResult()
    {
        var service = CreateService();
        _productRepository.Setup(x => x.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _productRepository.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Product>
        {
            new() { Name = "P1", Description = "D1", Price = 1, StockQuantity = 1 },
            new() { Name = "P2", Description = "D2", Price = 2, StockQuantity = 2 }
        });

        var result = await service.GetPagedAsync(new PaginationRequest(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
    }

    private ProductService CreateService()
    {
        return new ProductService(_productRepository.Object, _createValidator, _updateValidator);
    }
}
