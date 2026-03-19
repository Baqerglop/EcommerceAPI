using System.Security.Claims;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers;

// ── Auth ──────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Register a new customer account.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request);
        return Ok(result);
    }

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>Get current authenticated user info.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
        Name = User.FindFirstValue(ClaimTypes.Name),
        Email = User.FindFirstValue(ClaimTypes.Email),
        Role = User.FindFirstValue(ClaimTypes.Role)
    });
}

// ── Categories ────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _svc;
    public CategoriesController(ICategoryService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var result = await _svc.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryRequest request) =>
        Ok(await _svc.UpdateAsync(id, request));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return Ok(new MessageResponse("Category deleted."));
    }
}

// ── Products ──────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    /// <summary>Get all products with optional filtering and pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null)
        => Ok(await _svc.GetAllAsync(page, pageSize, search, categoryId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(ProductRequest request)
    {
        var result = await _svc.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProductRequest request) =>
        Ok(await _svc.UpdateAsync(id, request));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return Ok(new MessageResponse("Product deleted."));
    }
}

// ── Cart ──────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _svc;
    public CartController(ICartService svc) => _svc = svc;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart() => Ok(await _svc.GetCartAsync(UserId));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddToCartRequest request) =>
        Ok(await _svc.AddItemAsync(UserId, request));

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, UpdateCartRequest request) =>
        Ok(await _svc.UpdateItemAsync(UserId, cartItemId, request));

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId) =>
        Ok(await _svc.RemoveItemAsync(UserId, cartItemId));

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _svc.ClearCartAsync(UserId);
        return Ok(new MessageResponse("Cart cleared."));
    }
}

// ── Orders ────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    public OrdersController(IOrderService svc) => _svc = svc;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin =>
        User.IsInRole("Admin");

    /// <summary>Place a new order from the current cart.</summary>
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request) =>
        Ok(await _svc.PlaceOrderAsync(UserId, request));

    /// <summary>Get the authenticated user's order history.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders() =>
        Ok(await _svc.GetUserOrdersAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _svc.GetOrderByIdAsync(id, UserId, IsAdmin));

    /// <summary>Admin: Get all orders across all users.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _svc.GetAllOrdersAsync());

    /// <summary>Admin: Update the status of an order.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request) =>
        Ok(await _svc.UpdateStatusAsync(id, request));
}
