namespace EcommerceAPI.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────
public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Name, string Email, string Role);

// ── Category ──────────────────────────────────────────────────────────────────
public record CategoryRequest(string Name, string Description);
public record CategoryResponse(int Id, string Name, string Description, int ProductCount);

// ── Product ───────────────────────────────────────────────────────────────────
public record ProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string ImageUrl,
    int CategoryId
);

public record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string ImageUrl,
    bool IsActive,
    DateTime CreatedAt,
    int CategoryId,
    string CategoryName
);

// ── Cart ──────────────────────────────────────────────────────────────────────
public record AddToCartRequest(int ProductId, int Quantity);
public record UpdateCartRequest(int Quantity);

public record CartItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    string ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal
);

public record CartResponse(IEnumerable<CartItemResponse> Items, decimal Total);

// ── Order ─────────────────────────────────────────────────────────────────────
public record PlaceOrderRequest(string ShippingAddress);
public record UpdateOrderStatusRequest(string Status);

public record OrderItemResponse(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal
);

public record OrderResponse(
    int Id,
    string Status,
    decimal TotalAmount,
    string ShippingAddress,
    DateTime CreatedAt,
    IEnumerable<OrderItemResponse> Items
);

// ── Shared ────────────────────────────────────────────────────────────────────
public record PagedResult<T>(IEnumerable<T> Data, int Total, int Page, int PageSize);
public record MessageResponse(string Message);
