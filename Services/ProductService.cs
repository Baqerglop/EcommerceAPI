using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(int page, int pageSize, string? search, int? categoryId);
    Task<ProductResponse> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(ProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, ProductRequest request);
    Task DeleteAsync(int id);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<PagedResult<ProductResponse>> GetAllAsync(
        int page, int pageSize, string? search, int? categoryId)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search) || p.Description.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToResponse(p))
            .ToListAsync();

        return new PagedResult<ProductResponse>(data, total, page, pageSize);
    }

    public async Task<ProductResponse> GetByIdAsync(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        return ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new KeyNotFoundException("Category not found.");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(int id, ProductRequest request)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.ImageUrl = request.ImageUrl;
        product.CategoryId = request.CategoryId;

        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return ToResponse(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.IsActive = false; // Soft delete
        await _db.SaveChangesAsync();
    }

    private static ProductResponse ToResponse(Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.Stock,
        p.ImageUrl, p.IsActive, p.CreatedAt,
        p.CategoryId, p.Category?.Name ?? string.Empty
    );
}
