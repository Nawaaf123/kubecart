using Dapper;
using Catalog.Api.Data;
using Catalog.Api.Models;

namespace Catalog.Api.Data.Repositories;

public sealed class ProductRepository
{
    private readonly DapperContext _db;

    public ProductRepository(DapperContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ProductListDto>> GetProductsAsync(Guid? categoryId, string? search)
    {
        const string sql = """
        SELECT
            p.Id,
            p.CategoryId,
            p.Name,
            p.Price,
            p.StockQuantity
        FROM dbo.Products p
        WHERE (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@Search IS NULL OR p.Name LIKE '%' + @Search + '%')
        ORDER BY p.Name;
        """;

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<ProductListDto>(sql, new
        {
            CategoryId = categoryId,
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim()
        });
    }

    public async Task<ProductDetailDto?> GetByIdAsync(Guid id)
    {
        const string sqlProduct = """
        SELECT TOP 1
            p.Id,
            p.CategoryId,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity
        FROM dbo.Products p
        WHERE p.Id = @Id;
        """;

        const string sqlImages = """
        SELECT ImageUrl
        FROM dbo.ProductImages
        WHERE ProductId = @Id
        ORDER BY SortOrder;
        """;

        using var conn = _db.CreateConnection();

        var product = await conn.QuerySingleOrDefaultAsync<ProductDetailDto>(sqlProduct, new { Id = id });
        if (product == null) return null;

        var images = (await conn.QueryAsync<string>(sqlImages, new { Id = id })).ToList();
        product.Images = images;

        return product;
    }

    public async Task<IReadOnlyList<ProductListDto>> SearchAsync(
    string? query,
    int? categoryId)
    {
        var sql = """
        SELECT
            p.Id,
            p.Name,
            p.Slug,
            p.Price,
            p.StockQuantity,
            (
                SELECT TOP 1 ImageUrl
                FROM dbo.ProductImages i
                WHERE i.ProductId = p.Id
                ORDER BY i.SortOrder
            ) AS ImageUrl
        FROM dbo.Products p
        WHERE p.IsActive = 1
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (
                @Query IS NULL
                OR p.Name LIKE '%' + @Query + '%'
                OR p.Description LIKE '%' + @Query + '%'
          )
        ORDER BY p.CreatedAtUtc DESC;
    """;

        using var conn = _db.CreateConnection();

        var rows = await conn.QueryAsync<ProductListDto>(sql, new
        {
            Query = query,
            CategoryId = categoryId
        });

        return rows.ToList();
    }

    public async Task<bool> UpdateAsync(Guid id, int categoryId, string name, string? description, decimal price, int stockQuantity)
    {
        const string sql = """
        UPDATE dbo.Products
        SET CategoryId = @CategoryId,
            Name = @Name,
            Description = @Description,
            Price = @Price,
            StockQuantity = @StockQuantity,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE Id = @Id;
    """;

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(sql, new
        {
            Id = id,
            CategoryId = categoryId,
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // delete images first if you have FK constraints
        const string sql = """
        DELETE FROM dbo.ProductImages WHERE ProductId = @Id;
        DELETE FROM dbo.Products WHERE Id = @Id;
    """;

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }


    public async Task CreateAsync(
    Guid id,
    int categoryId,
    string name,
    string slug,
    string? description,
    decimal price,
    int stockQuantity
)
    {
        const string sql = """
    INSERT INTO dbo.Products
    (Id, CategoryId, Name, Slug, Description, Price, StockQuantity, IsActive, CreatedAtUtc, UpdatedAtUtc)
    VALUES
    (@Id, @CategoryId, @Name, @Slug, @Description, @Price, @StockQuantity, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    """;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            CategoryId = categoryId,
            Name = name,
            Slug = slug,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity
        });
    }

}
