# BOHUCO POS Backend

.NET Backend API for restaurant order management system.

## Overview

BOHUCO POS Backend is a robust ASP.NET Core Web API that powers the BOHUCO POS restaurant management system. It handles all server-side operations including tab/account management, order processing, and real-time communication with the frontend client.

## Tech Stack

- **Framework**: ASP.NET Core 10
- **Language**: C# 14
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core 10
- **Messaging**: MediatR (CQRS pattern)
- **Real-time**: SignalR Hubs
- **Database Provider**: Npgsql (PostgreSQL)

## Architecture

The project follows **Clean Architecture** principles with 4 distinct layers:

```
BohucoPos/
├── NexusPOS.API/              # Web API Layer
│   ├── Controllers/          # REST endpoints
│   ├── Hubs/                 # SignalR hubs
│   ├── Services/             # Application services
│   └── Program.cs            # Entry point & configuration
│
├── NexusPOS.Application/     # Application Layer
│   ├── Commands/             # Write operations (CQRS)
│   ├── Queries/              # Read operations (CQRS)
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Interfaces/           # Service contracts
│   └── Events/               # Domain events
│
├── NexusPOS.Domain/          # Domain Layer
│   ├── Entities/            # Business entities
│   ├── Enums/                # Domain enumerations
│   └── ValueObjects/         # Value objects
│
└── NexusPOS.Infrastructure/   # Infrastructure Layer
    ├── Data/                 # DbContext & configuration
    ├── Repositories/         # Data access implementations
    ├── UnitOfWork/           # Unit of Work pattern
    └── Services/             # Infrastructure services
```

## Domain Entities

### Tab
Represents a customer account/tab at a table or bar seat.

```csharp
public class Tab
{
    public int Id { get; private set; }
    public string Location { get; private set; }      // Table/Bar name (e.g., "Mesa 1", "Barra 2")
    public string CustomerName { get; private set; } // Customer name (required for bar)
    public TabStatus Status { get; set; }             // Open, Pending, Closed, Cancelled
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; set; }
    public PaymentMethod? PaymentMethod { get; set; } // Cash, Card, Transfer
    public decimal TaxRate { get; private set; }      // 18% ITBIS
    public ICollection<Order> Orders { get; }         // Related orders
    public decimal Subtotal => Orders.Sum(o => o.Items.Sum(i => i.UnitPrice * i.Quantity));
    public decimal Tax => Subtotal * TaxRate;
    public decimal Total => Subtotal + Tax;
}
```

### Order
Represents a single order placed by a customer.

```csharp
public class Order
{
    public int Id { get; private set; }
    public OrderType OrderType { get; private set; }  // Table, Bar
    public OrderStatus Status { get; private set; }  // Pending, Preparing, Ready, Delivered
    public int? TabId { get; set; }
    public string WaiterName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<OrderItem> Items { get; }
}
```

### OrderItem
Individual product items within an order.

```csharp
public class OrderItem
{
    public int Id { get; private set; }
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public string? Notes { get; private set; }
    public ItemDestination Destination { get; private set; } // Kitchen, Bar
    public ItemStatus Status { get; private set; }
}
```

## Enumerations

| Enum | Values |
|------|--------|
| **TabStatus** | Open, Pending, Closed, Cancelled |
| **OrderStatus** | Pending, Preparing, Ready, Delivered |
| **OrderType** | Table, Bar |
| **ItemDestination** | Kitchen, Bar |
| **ItemStatus** | Pending, Preparing, Ready, Served |
| **PaymentMethod** | Cash, Card, Transfer |

## API Endpoints

### Tabs Controller (`/api/tabs`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tabs/active` | Get all active tabs |
| GET | `/api/tabs/location/{location}` | Get tabs by location |
| GET | `/api/tabs/{id}` | Get tab details with orders |
| POST | `/api/tabs` | Open new tab |
| POST | `/api/tabs/{id}/request-bill` | Request bill (mark as pending) |
| POST | `/api/tabs/{id}/close` | Close tab with payment |
| POST | `/api/tabs/{id}/cancel` | Cancel tab |
| POST | `/api/tabs/orders` | Link order to tab |

### Orders Controller (`/api/orders`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders/table/{tableId}` | Get orders by table |
| GET | `/api/orders/destination/{destination}` | Get pending orders by destination |
| POST | `/api/orders` | Create new order |
| PATCH | `/api/orders/items/{itemId}/status` | Update item status |

### SignalR Hub (`/hubs/orders`)

Real-time order status updates for kitchen/bar displays.

**Events:**
- `OrderCreated` - New order placed
- `OrderUpdated` - Order updated
- `OrderItemStatusChanged` - Item status updated (notifies waiters)

### Authentication Controller (`/api/auth`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login and get JWT token |

### Products Controller (`/api/products`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products (filtered by destination) |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create new product (Admin) |
| PUT | `/api/products/{id}` | Update product (Admin) |
| DELETE | `/api/products/{id}` | Delete product (Admin) |

### Dashboard Controller (`/api/dashboard`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard/sales` | Sales analytics with date range |
| GET | `/api/dashboard/low-inventory` | Products with low stock |

Requires JWT authentication and Admin role.

### PDF Controller (`/api/pdf`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/pdf/bill/{tabId}` | Generate PDF bill for a tab |

Returns a PDF document with tab details, all orders, items, and totals. Requires JWT authentication.

### SignalR Hub (`/hubs/orders`) - JWT Auth

Real-time order status updates with JWT authentication.

**Connection:**
```
/hubs/orders?token=<jwt_token>
```

**Methods:**
- `JoinWaiterGroup(waiterName)` - Waiter joins their personal notification group
- `OrderItemStatusChanged(tabId, waiterName, itemName, newStatus)` - Broadcast to waiter's group

**Events received:**
- `OrderCreated` - New order placed
- `OrderUpdated` - Order updated
- `OrderItemStatusChanged` - Item status updated (notifies specific waiter)

### Role-Based Access Control

| Role | Access |
|------|--------|
| **Waiter** | Tabs, Orders |
| **Kitchen** | Order updates via SignalR |
| **Bar** | Order updates via SignalR |
| **Admin** | Products CRUD, Dashboard, PDF |

Protected endpoints require valid JWT token with matching role claim.

## Commands & Queries (MediatR)

### Commands
- `OpenTabCommand` - Create new account
- `CloseTabCommand` - Close account with payment
- `CancelTabCommand` - Cancel account
- `RequestBillCommand` - Mark as pending payment
- `CreateOrderCommand` - Create new order
- `AddOrderToTabCommand` - Add order to existing tab
- `UpdateOrderItemStatusCommand` - Update item status

### Queries
- `GetActiveTabsByLocationQuery` - Get active tabs
- `GetTabDetailsQuery` - Get tab with all orders
- `GetOpenTablesQuery` - Get tables with open tabs
- `GetOrdersByTableQuery` - Get orders by table
- `GetPendingOrdersByDestinationQuery` - Get pending items by Kitchen/Bar

## Database Configuration

The application uses PostgreSQL with Entity Framework Core.

### Connection String
```
Host=localhost;Database=nexuspos;Username=<your_user>;Password=<your_password>;Port=5434
```

### Docker Setup
```bash
docker run -d \
  --name nexuspos-db \
  -e POSTGRES_PASSWORD=<your_password> \
  -e POSTGRES_DB=nexuspos \
  -p 5434:5432 \
  postgres:16-alpine
```

### Migrations
To create and apply migrations:
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL 16+
- Node.js 18+ (for frontend)

### Configuration

Edit `NexusPOS.API/appsettings.json` to configure:
- Database connection string
- Logging levels
- CORS origins

### Running the API

```bash
# Restore dependencies
dotnet restore

# Run development server
dotnet run --project NexusPOS.API
```

The API runs at `https://localhost:7089`

### Running Tests
```bash
dotnet test
```

## Key Features

### Tab (Account) System
- Open tabs for tables or bar seats
- Track customer name (required for bar)
- Support for multiple orders per tab
- Request bill functionality
- Close with payment method (Cash/Card/Transfer)

### Order Management
- Create orders with multiple items
- Route items to Kitchen or Bar
- Track order status (Pending → Preparing → Ready → Delivered)
- Link orders to tabs automatically

### Item Destination
- Products can be routed to Kitchen or Bar
- Respects destination sent from frontend
- Fallback to routing service if not provided

### Real-time Updates
- SignalR hub with JWT authentication for live order status
- Kitchen and bar displays can subscribe to updates
- Waiters receive notifications when their order items are ready

### Role-Based Access
- JWT tokens contain role claims
- Waiter: Tabs and orders management
- Kitchen/Bar: View and update order items
- Admin: Products CRUD, Dashboard analytics, PDF generation

### PDF Bill Generation
- Generate professional PDF bills for closed tabs
- Includes all orders, items with quantities and notes
- Full tax breakdown (18% ITBIS)

### Dashboard & Analytics
- Sales analytics with date range filtering
- Low inventory alerts for products

## Project Structure Details

### NexusPOS.API
- REST controllers for Tabs and Orders
- SignalR hub for real-time communication
- CORS configuration for frontend access
- JSON serialization setup

### NexusPOS.Application
- MediatR handlers for all business operations
- Command and Query separation (CQRS)
- DTOs for API responses

### NexusPOS.Domain
- Core business entities with behavior
- Enumerations for type safety
- Domain logic encapsulated in entities

### NexusPOS.Infrastructure
- Entity Framework configuration
- Repository implementations
- Unit of Work pattern
- PostgreSQL database access

## Environment Variables

Can be configured via `appsettings.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=nexuspos;Username=<your_user>;Password=<your_password>;Port=5434"
  }
}
```

## Contributing

1. Create feature branch from `development`
2. Make changes following Clean Architecture
3. Ensure code compiles and tests pass
4. Create PR to `development` branch
5. Merge to `main` after review

## API Documentation

When running in development mode, OpenAPI documentation is available at:
- Swagger UI: `/swagger`
- OpenAPI JSON: `/openapi/v1.json`

## Version History

### [1.3.0] - 2026-03-18

**Added - PDF Bill Generation**
- New PDF controller with `/api/pdf/bill/{tabId}` endpoint
- Uses QuestPDF library to generate professional bills
- Includes tab details, all orders, items with notes, subtotal, tax, and total

**Added - Role-Based Access Control**
- JWT authentication with role claims (Waiter, Kitchen, Bar, Admin)
- SignalR JWT authentication support
- Admin-only endpoints for Products and Dashboard

**Added - Sales Analytics**
- Dashboard endpoint for sales analytics with date range filtering
- Low inventory report endpoint

**Fixed**
- SignalR waiter notifications now include Tab relationship for proper broadcasting

### [1.2.1] - 2026-03-11

**Added - Item Destination (Kitchen/Bar)**
- Added `Destination` field to `OrderItemDto` for frontend routing
- Updated `CreateOrderCommandHandler` to use destination from frontend when provided
- Added JSON enum converter to `Program.cs` for string-to-enum conversion
- Fixed Tab entity to use EF Core navigation for Orders relationship
- Added Tab navigation property to Order entity
- Configured Tab-Order relationship in `AppDbContext.cs`
- Removed manual sequence from Tab and uses database auto-increment

**Fixed**
- Payment Method Enum: Backend expects numeric enum values (0=Cash, 1=Card, 2=Transfer)
- Tab Entity ID Auto-Increment: Fixed duplicate key error when opening tabs

### [1.2.0] - 2026-03-11

**Added - Tab System (Account/Tab Management)**

Implemented full account/tab functionality:

- `Tab` entity with status (Open, Pending, Closed, Cancelled)
- `PaymentMethod` enum (Cash, Card, Transfer)
- New API endpoints:
  - POST `/api/tabs` - Open new tab
  - GET `/api/tabs/location/{location}` - Get active tabs by location
  - GET `/api/tabs/{tabId}` - Get tab details
  - GET `/api/tabs/active` - Get all active tabs
  - GET `/api/tabs/open` - Get all open tables with tab counts
  - POST `/api/tabs/orders` - Add order to tab
  - POST `/api/tabs/{id}/request-bill` - Request bill
  - POST `/api/tabs/{id}/close` - Close tab with payment
  - POST `/api/tabs/{id}/cancel` - Cancel tab

## Version

Current version: **1.3.0**

## License

Proprietary - BOHUCO Restaurant
