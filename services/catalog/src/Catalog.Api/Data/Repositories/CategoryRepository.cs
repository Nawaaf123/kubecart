using Dapper;
using Catalog.Api.Models;

namespace Catalog.Api.Data.Repositories;

public sealed class CategoryRepository
{
    private readonly DapperContext _db;

    public CategoryRepository(DapperContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        const string sql = """
            SELECT Id, Name, Slug
            FROM dbo.Categories
            WHERE IsActive = 1
            ORDER BY Name
        """;

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CategoryDto>(sql);
    }
}
