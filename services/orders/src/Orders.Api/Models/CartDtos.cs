namespace Orders.Api.Models;

public sealed class AddCartItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

public sealed class CartItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
