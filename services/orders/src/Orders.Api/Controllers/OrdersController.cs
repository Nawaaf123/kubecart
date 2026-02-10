using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Clients;
using Orders.Api.Data.Repositories;
using Orders.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly CatalogClient _catalog;
    private readonly CartRepository _cartRepo;
    private readonly OrderRepository _orderRepo;

    public OrdersController(CatalogClient catalog, CartRepository cartRepo, OrderRepository orderRepo)
    {
        _catalog = catalog;
        _cartRepo = cartRepo;
        _orderRepo = orderRepo;
    }

    // Read userId from JWT "sub"
    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(sub))
            throw new UnauthorizedAccessException("Missing sub claim in token.");

        return Guid.Parse(sub);
    }

    // -------------------- Order History --------------------

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();
        var orders = await _orderRepo.GetOrdersForUserAsync(userId);
        return Ok(orders);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMyOrder(Guid id)
    {
        var userId = GetUserId();
        var order = await _orderRepo.GetOrderDetailAsync(userId, id);
        return order == null ? NotFound() : Ok(order);
    }

    // -------------------- Cart --------------------

    [Authorize]
    [HttpGet("cart")]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        var items = await _cartRepo.GetCartAsync(userId);

        return Ok(items.Select(x => new CartItemDto
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity
        }));
    }

    [Authorize]
    [HttpPost("cart/items")]
    public async Task<IActionResult> AddToCart([FromBody] AddCartItemRequest req)
    {
        if (req.ProductId == Guid.Empty) return BadRequest("ProductId required.");
        if (req.Quantity <= 0) return BadRequest("Quantity must be > 0.");

        var userId = GetUserId();

        // validate product exists via Catalog API
        var product = await _catalog.GetProductAsync(req.ProductId);
        if (product == null) return NotFound("Product not found in catalog.");

        await _cartRepo.UpsertItemAsync(userId, req.ProductId, req.Quantity);
        return Ok(new { ok = true });
    }

    // -------------------- Checkout --------------------

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest req)
    {
        var userId = GetUserId();

        // 1) get cart
        var cart = await _cartRepo.GetCartAsync(userId);
        if (cart.Count == 0) return BadRequest("Cart is empty.");

        // 2) validate each product via catalog + compute totals
        var snapshots = new List<(Guid ProductId, string Name, decimal Price, string? ImageUrl, int Qty)>();
        decimal subtotal = 0;

        foreach (var (productId, qty) in cart)
        {
            var p = await _catalog.GetProductAsync(productId);
            if (p == null) return BadRequest($"Product not found: {productId}");

            if (p.StockQuantity < qty)
                return BadRequest($"Not enough stock for {p.Name}. Requested {qty}, Available {p.StockQuantity}");

            var image = p.Images.FirstOrDefault();
            snapshots.Add((productId, p.Name, p.Price, image, qty));
            subtotal += p.Price * qty;
        }

        var tax = req.Tax;
        var shipping = req.Shipping;
        var total = subtotal + tax + shipping;

        // 3) transaction: create order + items + clear cart
        using var conn = await _orderRepo.OpenConnectionAsync();
        using var tx = conn.BeginTransaction();

        try
        {
            var orderId = await _orderRepo.CreateOrderAsync(userId, subtotal, tax, shipping, total, tx);

            foreach (var s in snapshots)
            {
                await _orderRepo.AddOrderItemAsync(orderId, s.ProductId, s.Name, s.Price, s.ImageUrl, s.Qty, tx);
            }

            await _cartRepo.ClearCartAsync(userId, tx);

            tx.Commit();
            return Ok(new CheckoutResponse { OrderId = orderId, Total = total });
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // -------------------- Admin --------------------

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Status))
            return BadRequest("Status is required.");

        var ok = await _orderRepo.UpdateStatusAsync(orderId, req.Status.Trim());
        return ok ? Ok(new { ok = true }) : NotFound();
    }
}
