namespace Catalog.Api.Models;

public sealed class ProductListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
}
