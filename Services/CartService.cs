using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(int userId);
    Task<CartResponse> AddItemAsync(int userId, AddToCartRequest request);
    Task<CartResponse> UpdateItemAsync(int userId, int cartItemId, UpdateCartRequest request);
    Task<CartResponse> RemoveItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
}

public class CartService : ICartService
{
    private readonly AppDbContext _db;
    public CartService(AppDbContext db) => _db = db;

    public async Task<CartResponse> GetCartAsync(int userId)
    {
        var items = await GetCartItemsAsync(userId);
        return BuildResponse(items);
    }

    public async Task<CartResponse> AddItemAsync(int userId, AddToCartRequest request)
    {
        var product = await _db.Products.FindAsync(request.ProductId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (product.Stock < request.Quantity)
            throw new InvalidOperationException("Insufficient stock.");

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

        if (existing != null)
            existing.Quantity += request.Quantity;
        else
            _db.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });

        await _db.SaveChangesAsync();
        return BuildResponse(await GetCartItemsAsync(userId));
    }

    public async Task<CartResponse> UpdateItemAsync(int userId, int cartItemId, UpdateCartRequest request)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        if (request.Quantity <= 0)
            _db.CartItems.Remove(item);
        else
            item.Quantity = request.Quantity;

        await _db.SaveChangesAsync();
        return BuildResponse(await GetCartItemsAsync(userId));
    }

    public async Task<CartResponse> RemoveItemAsync(int userId, int cartItemId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return BuildResponse(await GetCartItemsAsync(userId));
    }

    public async Task ClearCartAsync(int userId)
    {
        var items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<List<CartItem>> GetCartItemsAsync(int userId) =>
        await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

    private static CartResponse BuildResponse(List<CartItem> items)
    {
        var dtos = items.Select(c => new CartItemResponse(
            c.Id,
            c.ProductId,
            c.Product!.Name,
            c.Product.ImageUrl,
            c.Product.Price,
            c.Quantity,
            c.Product.Price * c.Quantity
        ));
        return new CartResponse(dtos, dtos.Sum(i => i.SubTotal));
    }
}
