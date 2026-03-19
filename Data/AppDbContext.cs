using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Decimal precision
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        // Seed categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Gadgets and electronic devices" },
            new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion" },
            new Category { Id = 3, Name = "Books", Description = "Books and literature" },
            new Category { Id = 4, Name = "Home & Garden", Description = "Home and garden products" }
        );

        // Seed products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Wireless Headphones", Description = "Premium noise-cancelling headphones", Price = 199.99m, Stock = 50, CategoryId = 1, ImageUrl = "https://placehold.co/400x300?text=Headphones" },
            new Product { Id = 2, Name = "Mechanical Keyboard", Description = "RGB backlit mechanical keyboard", Price = 89.99m, Stock = 30, CategoryId = 1, ImageUrl = "https://placehold.co/400x300?text=Keyboard" },
            new Product { Id = 3, Name = "Classic T-Shirt", Description = "100% cotton comfortable t-shirt", Price = 19.99m, Stock = 100, CategoryId = 2, ImageUrl = "https://placehold.co/400x300?text=T-Shirt" },
            new Product { Id = 4, Name = "Clean Code", Description = "A Handbook of Agile Software Craftsmanship", Price = 34.99m, Stock = 25, CategoryId = 3, ImageUrl = "https://placehold.co/400x300?text=Book" }
        );
    }
}
