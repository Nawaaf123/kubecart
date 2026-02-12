using Catalog.Api.Data.Repositories;
using Catalog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/admin/products")]
[Authorize(Roles = "Admin")]
public sealed class AdminProductsController : ControllerBase
{
    private readonly ProductRepository _products;

    public AdminProductsController(ProductRepository products)
    {
        _products = products;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
        if (dto.Price < 0) return BadRequest("Price must be >= 0.");
        if (dto.StockQuantity < 0) return BadRequest("StockQuantity must be >= 0.");
        if (dto.CategoryId <= 0) return BadRequest("CategoryId must be a valid category.");

        var id = Guid.NewGuid();
        var name = dto.Name.Trim();
        var slug = Slugify(name);

        await _products.CreateAsync(
            id,
            dto.CategoryId,
            name,
            slug,
            string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            dto.Price,
            dto.StockQuantity
        );

        return Ok(new { productId = id, slug });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
        if (dto.Price < 0) return BadRequest("Price must be >= 0.");
        if (dto.StockQuantity < 0) return BadRequest("StockQuantity must be >= 0.");
        if (dto.CategoryId <= 0) return BadRequest("CategoryId must be a valid category.");

        var ok = await _products.UpdateAsync(
            id,
            dto.CategoryId,
            dto.Name.Trim(),
            string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            dto.Price,
            dto.StockQuantity
        );

        return ok ? Ok(new { ok = true }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _products.DeleteAsync(id);
        return ok ? Ok(new { ok = true }) : NotFound();
    }

    private static string Slugify(string text)
    {
        text = text.Trim().ToLowerInvariant();

        var chars = text.Select(c =>
            char.IsLetterOrDigit(c) ? c :
            char.IsWhiteSpace(c) ? '-' : '\0'
        ).Where(c => c != '\0').ToArray();

        var slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
