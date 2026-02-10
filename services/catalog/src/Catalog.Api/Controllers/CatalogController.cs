using Microsoft.AspNetCore.Mvc;
using Catalog.Api.Data.Repositories;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly CategoryRepository _categories;
    private readonly ProductRepository _products;

    public CatalogController(
        CategoryRepository categories,
        ProductRepository products)
    {
        _categories = categories;
        _products = products;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categories.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] int? categoryId,
        [FromQuery] string? search)
    {
        var result = await _products.SearchAsync(categoryId, search);
        return Ok(result);
    }

    [HttpGet("products/{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }
}
