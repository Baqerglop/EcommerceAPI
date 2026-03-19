using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> GetByIdAsync(int id);
    Task<CategoryResponse> CreateAsync(CategoryRequest request);
    Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request);
    Task DeleteAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync() =>
        await _db.Categories
            .Select(c => new CategoryResponse(
                c.Id, c.Name, c.Description,
                c.Products.Count(p => p.IsActive)))
            .ToListAsync();

    public async Task<CategoryResponse> GetByIdAsync(int id)
    {
        var c = await _db.Categories.Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        return new CategoryResponse(c.Id, c.Name, c.Description,
            c.Products.Count(p => p.IsActive));
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request)
    {
        var cat = new Category { Name = request.Name, Description = request.Description };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return new CategoryResponse(cat.Id, cat.Name, cat.Description, 0);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request)
    {
        var cat = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        cat.Name = request.Name;
        cat.Description = request.Description;
        await _db.SaveChangesAsync();

        return new CategoryResponse(cat.Id, cat.Name, cat.Description,
            await _db.Products.CountAsync(p => p.CategoryId == id && p.IsActive));
    }

    public async Task DeleteAsync(int id)
    {
        var cat = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        if (await _db.Products.AnyAsync(p => p.CategoryId == id && p.IsActive))
            throw new InvalidOperationException("Cannot delete a category that has active products.");

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
    }
}
