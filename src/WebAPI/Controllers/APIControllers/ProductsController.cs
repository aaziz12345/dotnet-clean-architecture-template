using Application.Common.Models;
using Application.DTOs.Product;
using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetPagedAsync(new PaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to fetch products."));
        }

        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Product not found."));
        }

        return Ok(ApiResponse<ProductDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to create product."));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<ProductDto>.Ok(result.Value!, "Product created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Product not found."));
        }

        return Ok(ApiResponse<ProductDto>.Ok(result.Value!, "Product updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Product not found."));
        }

        return Ok(ApiResponse.Ok("Product deleted."));
    }
}
