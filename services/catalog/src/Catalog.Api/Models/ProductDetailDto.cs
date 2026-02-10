namespace Catalog.Api.Models;

public sealed class ProductDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public List<string> Images { get; init; } = new();
}
