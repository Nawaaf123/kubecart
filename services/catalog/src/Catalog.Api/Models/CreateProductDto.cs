namespace Catalog.Api.Models;

public sealed class CreateProductDto
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
