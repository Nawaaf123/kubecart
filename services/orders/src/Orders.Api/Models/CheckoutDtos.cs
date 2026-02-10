namespace Orders.Api.Models;

public sealed class CheckoutRequest
{
    // For now we keep it simple (no address/payment yet)
    public decimal Tax { get; init; } = 0;
    public decimal Shipping { get; init; } = 0;
}

public sealed class CheckoutResponse
{
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
}
