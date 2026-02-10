using Dapper;
using System.Data;


namespace Orders.Api.Data.Repositories;

public sealed class CartRepository
{
    private readonly DapperContext _db;

    public CartRepository(DapperContext db) => _db = db;

    public async Task UpsertItemAsync(Guid userId, Guid productId, int quantity)
    {
        const string sql = """
        MERGE dbo.CartItems AS target
        USING (SELECT @UserId AS UserId, @ProductId AS ProductId) AS src
        ON target.UserId = src.UserId AND target.ProductId = src.ProductId
        WHEN MATCHED THEN
            UPDATE SET Quantity = @Quantity, UpdatedAtUtc = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT (Id, UserId, ProductId, Quantity, CreatedAtUtc)
            VALUES (NEWID(), @UserId, @ProductId, @Quantity, SYSUTCDATETIME());
        """;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(sql, new { UserId = userId, ProductId = productId, Quantity = quantity });
    }

    public async Task<List<(Guid ProductId, int Quantity)>> GetCartAsync(Guid userId)
    {
        const string sql = """
        SELECT ProductId, Quantity
        FROM dbo.CartItems
        WHERE UserId = @UserId
        ORDER BY CreatedAtUtc;
        """;

        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync(sql, new { UserId = userId });
        return rows.Select(r => ((Guid)r.ProductId, (int)r.Quantity)).ToList();
    }

    public async Task ClearCartAsync(Guid userId, System.Data.IDbTransaction tx)
    {
        const string sql = "DELETE FROM dbo.CartItems WHERE UserId = @UserId;";
        await tx.Connection!.ExecuteAsync(sql, new { UserId = userId }, tx);
    }
}
