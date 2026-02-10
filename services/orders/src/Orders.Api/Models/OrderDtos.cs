namespace Orders.Api.Models;

public sealed class OrderListDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string? ImageUrl { get; init; }
    public int Quantity { get; init; }

    // Optional: If you want to show computed LineTotal from DB
    public decimal? LineTotal { get; init; }
}

public sealed class OrderDetailDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal Tax { get; init; }
    public decimal Shipping { get; init; }
    public decimal Total { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }   // matches your schema

    public List<OrderItemDto> Items { get; init; } = new();
}

public sealed class UpdateOrderStatusRequest
{
    public string Status { get; init; } = string.Empty;
}
