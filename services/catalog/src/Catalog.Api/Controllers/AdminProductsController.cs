using Catalog.Api.Data.Repositories;
using Catalog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/admin/products")]
[Authorize(Roles = "Admin")] // 🔐 ADMIN ONLY
public sealed class AdminProductsController : ControllerBase
{
    private readonly ProductRepository _products;

    public AdminProductsController(ProductRepository products)
    {
        _products = products;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Product name is required.");

        var id = Guid.NewGuid();

        await _products.CreateAsync(
            id,
            dto.Name,
            dto.Price,
            dto.StockQuantity
        );

        return Ok(new { productId = id });
    }
}
