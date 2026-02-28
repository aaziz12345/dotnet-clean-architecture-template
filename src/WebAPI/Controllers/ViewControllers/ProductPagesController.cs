using Application.Common.Models;
using Application.DTOs.Product;
using Application.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ViewModels.Product;

namespace WebAPI.Controllers;

[Authorize]
[Route("products")]
public sealed class ProductPagesController : Controller
{
    private readonly IProductService _productService;

    public ProductPagesController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetPagedAsync(new PaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error ?? "Failed to load products.";
            return View(new ProductListViewModel
            {
                Products = Array.Empty<ProductDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            });
        }

        return View(new ProductListViewModel
        {
            Products = result.Value!.Items,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            TotalCount = result.Value.TotalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error ?? "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("create")]
    public IActionResult Create() => View(new ProductFormViewModel());

    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _productService.CreateAsync(new CreateProductRequestDto
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity
            }, cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create product.");
                return View(model);
            }

            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error ?? "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ProductFormViewModel
        {
            Id = result.Value!.Id,
            Name = result.Value.Name,
            Description = result.Value.Description,
            Price = result.Value.Price,
            StockQuantity = result.Value.StockQuantity
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _productService.UpdateAsync(id, new UpdateProductRequestDto
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity
            }, cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to update product.");
                return View(model);
            }

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(id, cancellationToken);
        TempData[result.IsFailure ? "Error" : "Success"] = result.Error ?? "Product deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
