using Dapper;
using Catalog.Api.Models;

namespace Catalog.Api.Data.Repositories;

public sealed class ProductRepository
{
    private readonly DapperContext _db;

    public ProductRepository(DapperContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ProductListDto>> SearchAsync(int? categoryId, string? search)
    {
        const string sql = """
            SELECT
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                pi.ImageUrl
            FROM dbo.Products p
            LEFT JOIN dbo.ProductImages pi ON pi.ProductId = p.Id AND pi.SortOrder = 0
            WHERE p.IsActive = 1
              AND (@categoryId IS NULL OR p.CategoryId = @categoryId)
              AND (@search IS NULL OR p.Name LIKE '%' + @search + '%')
            ORDER BY p.Name
        """;

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<ProductListDto>(sql, new { categoryId, search });
    }

    public async Task<ProductDetailDto?> GetByIdAsync(Guid id)
    {
        const string productSql = """
            SELECT Id, Name, Description, Price, StockQuantity
            FROM dbo.Products
            WHERE Id = @id AND IsActive = 1
        """;

        const string imagesSql = """
            SELECT ImageUrl
            FROM dbo.ProductImages
            WHERE ProductId = @id
            ORDER BY SortOrder
        """;

        using var conn = _db.CreateConnection();

        var product = await conn.QuerySingleOrDefaultAsync<ProductDetailDto>(productSql, new { id });
        if (product == null) return null;

        var images = await conn.QueryAsync<string>(imagesSql, new { id });
        product.Images.AddRange(images);

        return product;
    }
}
