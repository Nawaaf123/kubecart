using System.Net.Http.Json;

namespace Orders.Api.Clients;

public sealed class CatalogClient
{
    private readonly HttpClient _http;

    public CatalogClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<CatalogProduct?> GetProductAsync(Guid productId, CancellationToken ct = default)
    {
        // Catalog endpoint: GET /api/catalog/products/{id}
        return await _http.GetFromJsonAsync<CatalogProduct>($"/api/catalog/products/{productId}", ct);
    }
}

public sealed class CatalogProduct
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public List<string> Images { get; init; } = new();
}
