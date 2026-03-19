using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services;

public interface IOrderService
{
    Task<OrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request);
    Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId);
    Task<OrderResponse> GetOrderByIdAsync(int orderId, int userId, bool isAdmin);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(); // Admin
    Task<OrderResponse> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request); // Admin
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly ICartService _cartService;

    private static readonly string[] ValidStatuses =
        ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"];

    public OrderService(AppDbContext db, ICartService cartService)
    {
        _db = db;
        _cartService = cartService;
    }

    public async Task<OrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request)
    {
        var cartItems = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
            throw new InvalidOperationException("Your cart is empty.");

        // Validate stock
        foreach (var item in cartItems)
        {
            if (item.Product!.Stock < item.Quantity)
                throw new InvalidOperationException(
                    $"'{item.Product.Name}' has only {item.Product.Stock} items left.");
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = request.ShippingAddress,
            TotalAmount = cartItems.Sum(c => c.Product!.Price * c.Quantity),
            Items = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Product!.Price
            }).ToList()
        };

        // Deduct stock
        foreach (var item in cartItems)
            item.Product!.Stock -= item.Quantity;

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems); // Clear cart
        await _db.SaveChangesAsync();

        return await GetOrderResponseAsync(order.Id);
    }

    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId)
    {
        var orderIds = await _db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.Id)
            .ToListAsync();

        var result = new List<OrderResponse>();
        foreach (var id in orderIds)
            result.Add(await GetOrderResponseAsync(id));

        return result;
    }

    public async Task<OrderResponse> GetOrderByIdAsync(int orderId, int userId, bool isAdmin)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!isAdmin && order.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return await GetOrderResponseAsync(orderId);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
    {
        var orderIds = await _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.Id)
            .ToListAsync();

        var result = new List<OrderResponse>();
        foreach (var id in orderIds)
            result.Add(await GetOrderResponseAsync(id));

        return result;
    }

    public async Task<OrderResponse> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        if (!ValidStatuses.Contains(request.Status))
            throw new ArgumentException($"Invalid status. Valid values: {string.Join(", ", ValidStatuses)}");

        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetOrderResponseAsync(orderId);
    }

    // ── Helper ─────────────────────────────────────────────────────────────────

    private async Task<OrderResponse> GetOrderResponseAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == orderId);

        return new OrderResponse(
            order.Id,
            order.Status,
            order.TotalAmount,
            order.ShippingAddress,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemResponse(
                i.ProductId,
                i.Product!.Name,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity
            ))
        );
    }
}
