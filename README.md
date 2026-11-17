# InventoryHub

InventoryHub is an inventory / stock management admin dashboard built to demonstrate
professional, production-style **.NET** development. It's a working ASP.NET Core MVC
application backed by EF Core (SQLite) with a reactive client-side layer built on
**Knockout.js** — showcasing the kind of pre-SPA-framework enterprise UI pattern still
common in real-world ASP.NET shops, alongside modern .NET backend practices.

## Tech Stack

- **ASP.NET Core MVC** (.NET 10, C#)
- **Entity Framework Core** with the **SQLite** provider (file-based database, no external DB server required)
- **Knockout.js** (via CDN) for observable-based, reactive client-side data binding — no full-page reloads for CRUD operations
- **Bootstrap 5** for a clean, professional admin dashboard layout
- Minimal JSON API controllers (`/api/products`, `/api/categories`, `/api/stockmovements`) that the Knockout view models consume

## Features

- **Dashboard overview** with live stats: total products, total stock value, low-stock item count, and category count
- **Reactive product table** with instant client-side search (by name/SKU) and category filtering — no server round trip
- **Low-stock highlighting**: rows and badges turn red whenever `QuantityInStock` drops below a product's `ReorderThreshold`
- **Inline stock adjustment**: quick +/- buttons for one-unit changes, plus a full "Adjust Stock" modal supporting custom amounts and a reason (`Restock`, `Sale`, `Adjustment`, `Damaged`) — every adjustment is recorded as a `StockMovement`
- **Product CRUD**: add, edit, and delete products through modal forms, fully wired to the JSON API with server-side validation (unique SKU, valid category, non-negative price/threshold)
- **Category management** via the API (categories seeded out of the box; deletion is blocked while products still reference a category)
- **EF Core Code-First migrations** with a realistic seed of ~18 products across 4 categories (Electronics, Office Supplies, Warehouse & Tools, Furniture), including sample stock movement history
- Custom Knockout view models and bindings implemented in a dedicated `wwwroot/js/inventory.js` file (no inline script in Razor views)

## Screenshots

_Screenshots coming soon — run the project locally to see the live dashboard._

## Project Structure

```
InventoryHub/
├── Controllers/
│   ├── HomeController.cs          # Serves the Razor shell (dashboard + about pages)
│   └── Api/                       # JSON API controllers consumed by Knockout
│       ├── ProductsApiController.cs
│       ├── CategoriesApiController.cs
│       └── StockMovementsApiController.cs
├── Data/
│   └── InventoryContext.cs        # EF Core DbContext + seed data
├── Migrations/                    # EF Core Code-First migrations
├── Models/
│   ├── Category.cs, Product.cs, StockMovement.cs
│   └── Dtos.cs                    # API request/response DTOs
├── Views/
│   ├── Home/Index.cshtml          # Dashboard shell with Knockout data-bind markup
│   └── Shared/_Layout.cshtml      # Sidebar admin layout
└── wwwroot/js/inventory.js        # Knockout view models and bindings
```

## Setup & Run

Prerequisites: [.NET SDK 10](https://dotnet.microsoft.com/download) and the `dotnet-ef` tool.

```bash
# 1. Restore dependencies
dotnet restore

# 2. Install the EF Core CLI tool if you don't already have it
dotnet tool install --global dotnet-ef

# 3. Apply migrations and create the SQLite database (with seed data)
dotnet ef database update

# 4. Run the app
dotnet run
```

Then open `http://localhost:5234` (or `https://localhost:7252` with
`dotnet run --launch-profile https`). The database is also migrated automatically on startup
(`Program.cs` calls `db.Database.Migrate()`), so a fresh clone will "just work" even
if you skip step 3.

## Why Knockout.js?

React and Vue dominate greenfield projects, but a large share of enterprise ASP.NET
applications still run on Knockout.js, jQuery, or similar pre-SPA-framework patterns.
This project intentionally uses Knockout to demonstrate comfort working in that kind
of codebase — observable view models, declarative `data-bind` markup, computed
properties, and manual API wiring — while still following modern ASP.NET Core MVC and
EF Core backend conventions.

## License

MIT — see [LICENSE](LICENSE).
