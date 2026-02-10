using System.Data;
using Dapper;
using Orders.Api.Models;

namespace Orders.Api.Data.Repositories;

public sealed class OrderRepository
{
    private readonly DapperContext _db;

    public OrderRepository(DapperContext db)
    {
        _db = db;
    }

    // Used by checkout transaction code (you already have this pattern)
    public async Task<IDbConnection> OpenConnectionAsync()
    {
        var conn = _db.CreateConnection();
        await ((Microsoft.Data.SqlClient.SqlConnection)conn).OpenAsync();
        return conn;
    }

    public async Task<Guid> CreateOrderAsync(
        Guid userId,
        decimal subtotal,
        decimal tax,
        decimal shipping,
        decimal total,
        IDbTransaction tx)
    {
        var orderId = Guid.NewGuid();

        const string sql = """
            INSERT INTO dbo.Orders
                (Id, UserId, Status, Subtotal, Tax, Shipping, Total, CreatedAtUtc)
            VALUES
                (@OrderId, @UserId, 'Pending', @Subtotal, @Tax, @Shipping, @Total, SYSUTCDATETIME());
        """;

        await tx.Connection!.ExecuteAsync(sql, new
        {
            OrderId = orderId,
            UserId = userId,
            Subtotal = subtotal,
            Tax = tax,
            Shipping = shipping,
            Total = total
        }, tx);

        return orderId;
    }

    public async Task AddOrderItemAsync(
        Guid orderId,
        Guid productId,
        string productName,
        decimal unitPrice,
        string? imageUrl,
        int quantity,
        IDbTransaction tx)
    {
        // LineTotal is computed in DB, do NOT insert it
        const string sql = """
            INSERT INTO dbo.OrderItems
                (Id, OrderId, ProductId, ProductName, UnitPrice, ImageUrl, Quantity)
            VALUES
                (NEWID(), @OrderId, @ProductId, @ProductName, @UnitPrice, @ImageUrl, @Quantity);
        """;

        await tx.Connection!.ExecuteAsync(sql, new
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            ImageUrl = imageUrl,
            Quantity = quantity
        }, tx);
    }

    // -------------------- NEW: Order history --------------------

    public async Task<IEnumerable<OrderListDto>> GetOrdersForUserAsync(Guid userId)
    {
        const string sql = """
            SELECT Id, Status, Total, CreatedAtUtc
            FROM dbo.Orders
            WHERE UserId = @UserId
            ORDER BY CreatedAtUtc DESC;
        """;

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<OrderListDto>(sql, new { UserId = userId });
    }

    public async Task<OrderDetailDto?> GetOrderDetailAsync(Guid userId, Guid orderId)
    {
        const string orderSql = """
            SELECT Id, Status, Subtotal, Tax, Shipping, Total, CreatedAtUtc, UpdatedAtUtc
            FROM dbo.Orders
            WHERE Id = @OrderId AND UserId = @UserId;
        """;

        const string itemsSql = """
            SELECT ProductId, ProductName, UnitPrice, ImageUrl, Quantity, LineTotal
            FROM dbo.OrderItems
            WHERE OrderId = @OrderId
            ORDER BY ProductName;
        """;

        using var conn = _db.CreateConnection();

        var order = await conn.QuerySingleOrDefaultAsync<OrderDetailDto>(
            orderSql, new { UserId = userId, OrderId = orderId });

        if (order == null) return null;

        var items = await conn.QueryAsync<OrderItemDto>(itemsSql, new { OrderId = orderId });
        order.Items.AddRange(items);

        return order;
    }

    // -------------------- NEW: Admin status update --------------------

    public async Task<bool> UpdateStatusAsync(Guid orderId, string status)
    {
        const string sql = """
            UPDATE dbo.Orders
            SET Status = @Status,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE Id = @OrderId;
        """;

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(sql, new { OrderId = orderId, Status = status });
        return rows > 0;
    }
}
