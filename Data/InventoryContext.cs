using InventoryHub.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Seed data: categories ---
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Office Supplies" },
                new Category { Id = 3, Name = "Warehouse & Tools" },
                new Category { Id = 4, Name = "Furniture" }
            );

            // --- Seed data: products (~18 items across 4 categories) ---
            modelBuilder.Entity<Product>().HasData(
                // Electronics
                new Product { Id = 1, Name = "Wireless Mouse MX200", SKU = "ELEC-001", CategoryId = 1, Price = 24.99m, QuantityInStock = 85, ReorderThreshold = 20 },
                new Product { Id = 2, Name = "Mechanical Keyboard K6", SKU = "ELEC-002", CategoryId = 1, Price = 59.99m, QuantityInStock = 42, ReorderThreshold = 15 },
                new Product { Id = 3, Name = "27\" LED Monitor", SKU = "ELEC-003", CategoryId = 1, Price = 189.00m, QuantityInStock = 12, ReorderThreshold = 10 },
                new Product { Id = 4, Name = "USB-C Docking Station", SKU = "ELEC-004", CategoryId = 1, Price = 89.50m, QuantityInStock = 7, ReorderThreshold = 12 },
                new Product { Id = 5, Name = "1080p Webcam", SKU = "ELEC-005", CategoryId = 1, Price = 34.99m, QuantityInStock = 60, ReorderThreshold = 20 },
                // Office Supplies
                new Product { Id = 6, Name = "A4 Printer Paper (500 sheets)", SKU = "OFF-001", CategoryId = 2, Price = 6.49m, QuantityInStock = 220, ReorderThreshold = 50 },
                new Product { Id = 7, Name = "Blue Ballpoint Pens (Box of 12)", SKU = "OFF-002", CategoryId = 2, Price = 3.99m, QuantityInStock = 150, ReorderThreshold = 40 },
                new Product { Id = 8, Name = "Sticky Notes Pack", SKU = "OFF-003", CategoryId = 2, Price = 4.25m, QuantityInStock = 18, ReorderThreshold = 25 },
                new Product { Id = 9, Name = "Stapler Heavy Duty", SKU = "OFF-004", CategoryId = 2, Price = 12.99m, QuantityInStock = 33, ReorderThreshold = 10 },
                new Product { Id = 10, Name = "Manila Folders (Box of 100)", SKU = "OFF-005", CategoryId = 2, Price = 15.75m, QuantityInStock = 5, ReorderThreshold = 15 },
                // Warehouse & Tools
                new Product { Id = 11, Name = "Cordless Drill 18V", SKU = "WH-001", CategoryId = 3, Price = 79.99m, QuantityInStock = 24, ReorderThreshold = 8 },
                new Product { Id = 12, Name = "Safety Gloves (Pair)", SKU = "WH-002", CategoryId = 3, Price = 8.50m, QuantityInStock = 95, ReorderThreshold = 30 },
                new Product { Id = 13, Name = "Pallet Wrap Roll", SKU = "WH-003", CategoryId = 3, Price = 22.00m, QuantityInStock = 14, ReorderThreshold = 15 },
                new Product { Id = 14, Name = "Heavy Duty Shelving Unit", SKU = "WH-004", CategoryId = 3, Price = 145.00m, QuantityInStock = 6, ReorderThreshold = 5 },
                new Product { Id = 15, Name = "Digital Luggage Scale", SKU = "WH-005", CategoryId = 3, Price = 17.99m, QuantityInStock = 40, ReorderThreshold = 10 },
                // Furniture
                new Product { Id = 16, Name = "Ergonomic Office Chair", SKU = "FUR-001", CategoryId = 4, Price = 210.00m, QuantityInStock = 9, ReorderThreshold = 6 },
                new Product { Id = 17, Name = "Standing Desk (Electric)", SKU = "FUR-002", CategoryId = 4, Price = 380.00m, QuantityInStock = 4, ReorderThreshold = 5 },
                new Product { Id = 18, Name = "2-Drawer Filing Cabinet", SKU = "FUR-003", CategoryId = 4, Price = 95.00m, QuantityInStock = 11, ReorderThreshold = 8 }
            );

            // --- Seed data: a few initial stock movements for history ---
            var seedDate = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<StockMovement>().HasData(
                new StockMovement { Id = 1, ProductId = 1, ChangeAmount = 100, Reason = StockMovementReason.Restock, Timestamp = seedDate },
                new StockMovement { Id = 2, ProductId = 1, ChangeAmount = -15, Reason = StockMovementReason.Sale, Timestamp = seedDate.AddDays(3) },
                new StockMovement { Id = 3, ProductId = 3, ChangeAmount = 20, Reason = StockMovementReason.Restock, Timestamp = seedDate.AddDays(1) },
                new StockMovement { Id = 4, ProductId = 3, ChangeAmount = -8, Reason = StockMovementReason.Sale, Timestamp = seedDate.AddDays(5) },
                new StockMovement { Id = 5, ProductId = 10, ChangeAmount = 20, Reason = StockMovementReason.Restock, Timestamp = seedDate.AddDays(1) },
                new StockMovement { Id = 6, ProductId = 10, ChangeAmount = -15, Reason = StockMovementReason.Sale, Timestamp = seedDate.AddDays(10) },
                new StockMovement { Id = 7, ProductId = 17, ChangeAmount = -1, Reason = StockMovementReason.Damaged, Timestamp = seedDate.AddDays(12) },
                new StockMovement { Id = 8, ProductId = 8, ChangeAmount = -7, Reason = StockMovementReason.Adjustment, Timestamp = seedDate.AddDays(14) }
            );
        }
    }
}
