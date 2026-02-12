namespace Catalog.Api.Models;

public sealed class ProductDetailDto
{
    public Guid Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    // 🔴 CHANGE THIS LINE
    public List<string> Images { get; set; } = new();
}

