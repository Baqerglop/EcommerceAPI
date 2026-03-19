# 🛒 Ecommerce API — ASP.NET Core 8 + SQLite

A production-ready e-commerce REST API with authentication, products, cart, and orders.

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

### Run the API

```bash
cd EcommerceAPI
dotnet run
```

Then open your browser at: **http://localhost:5000** (Swagger UI loads at root `/`)

---

## 🗂 Project Structure

```
EcommerceAPI/
├── Controllers/        # API endpoints (Auth, Products, Categories, Cart, Orders)
├── Services/           # Business logic layer
├── Models/             # EF Core entity models
├── DTOs/               # Request/response data transfer objects
├── Data/               # AppDbContext + EF configuration
├── Middleware/         # Global exception handler
├── Migrations/         # EF Core SQLite migrations
├── Program.cs          # App bootstrap & DI registration
└── appsettings.json    # Configuration (JWT, DB connection)
```

---

## 🔑 Authentication

| Method | Endpoint            | Description              |
|--------|---------------------|--------------------------|
| POST   | `/api/auth/register` | Register new account     |
| POST   | `/api/auth/login`   | Login & receive JWT token |
| GET    | `/api/auth/me`      | Get current user info    |

**Using the token:**
Add to request headers: `Authorization: Bearer <your-token>`

**Roles:** `Customer` (default) | `Admin`

> To create an Admin user, register normally then manually update the `Role` column in the SQLite database.

---

## 📦 API Endpoints

### Categories (Public read / Admin write)
| Method | Endpoint              | Auth     |
|--------|-----------------------|----------|
| GET    | `/api/categories`     | Public   |
| GET    | `/api/categories/{id}` | Public  |
| POST   | `/api/categories`     | Admin    |
| PUT    | `/api/categories/{id}` | Admin   |
| DELETE | `/api/categories/{id}` | Admin   |

### Products (Public read / Admin write)
| Method | Endpoint             | Auth   | Notes                              |
|--------|----------------------|--------|------------------------------------|
| GET    | `/api/products`      | Public | `?page=1&pageSize=10&search=&categoryId=` |
| GET    | `/api/products/{id}` | Public |                                    |
| POST   | `/api/products`      | Admin  |                                    |
| PUT    | `/api/products/{id}` | Admin  |                                    |
| DELETE | `/api/products/{id}` | Admin  | Soft delete                        |

### Cart (Authenticated)
| Method | Endpoint                  | Description              |
|--------|---------------------------|--------------------------|
| GET    | `/api/cart`               | View cart                |
| POST   | `/api/cart/items`         | Add item to cart         |
| PUT    | `/api/cart/items/{id}`    | Update item quantity     |
| DELETE | `/api/cart/items/{id}`    | Remove item from cart    |
| DELETE | `/api/cart`               | Clear entire cart        |

### Orders (Authenticated)
| Method | Endpoint               | Auth     | Description               |
|--------|------------------------|----------|---------------------------|
| POST   | `/api/orders`          | Customer | Checkout from cart        |
| GET    | `/api/orders/my`       | Customer | My order history          |
| GET    | `/api/orders/{id}`     | Customer | Get order by ID           |
| GET    | `/api/orders`          | Admin    | All orders                |
| PATCH  | `/api/orders/{id}/status` | Admin | Update order status       |

**Order Statuses:** `Pending` → `Processing` → `Shipped` → `Delivered` / `Cancelled`

---

## ⚙️ Configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=ecommerce.db"
  },
  "Jwt": {
    "Key": "CHANGE_THIS_TO_A_LONG_SECURE_RANDOM_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "EcommerceAPI",
    "Audience": "EcommerceClient"
  }
}
```

> ⚠️ **Important:** Change the `Jwt:Key` to a strong random secret before deploying!

---

## 🌱 Seed Data

The database is automatically seeded with:
- **4 Categories:** Electronics, Clothing, Books, Home & Garden
- **4 Products:** Wireless Headphones, Mechanical Keyboard, Classic T-Shirt, Clean Code book

---

## 🛠 Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web framework |
| Entity Framework Core 8 | ORM |
| SQLite | Database |
| JWT Bearer | Authentication |
| BCrypt.Net | Password hashing |
| Swagger / OpenAPI | API documentation |
