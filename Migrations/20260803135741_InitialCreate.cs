using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryHub.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    SKU = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityInStock = table.Column<int>(type: "INTEGER", nullable: false),
                    ReorderThreshold = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Electronics" },
                    { 2, "Office Supplies" },
                    { 3, "Warehouse & Tools" },
                    { 4, "Furniture" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Name", "Price", "QuantityInStock", "ReorderThreshold", "SKU" },
                values: new object[,]
                {
                    { 1, 1, "Wireless Mouse MX200", 24.99m, 85, 20, "ELEC-001" },
                    { 2, 1, "Mechanical Keyboard K6", 59.99m, 42, 15, "ELEC-002" },
                    { 3, 1, "27\" LED Monitor", 189.00m, 12, 10, "ELEC-003" },
                    { 4, 1, "USB-C Docking Station", 89.50m, 7, 12, "ELEC-004" },
                    { 5, 1, "1080p Webcam", 34.99m, 60, 20, "ELEC-005" },
                    { 6, 2, "A4 Printer Paper (500 sheets)", 6.49m, 220, 50, "OFF-001" },
                    { 7, 2, "Blue Ballpoint Pens (Box of 12)", 3.99m, 150, 40, "OFF-002" },
                    { 8, 2, "Sticky Notes Pack", 4.25m, 18, 25, "OFF-003" },
                    { 9, 2, "Stapler Heavy Duty", 12.99m, 33, 10, "OFF-004" },
                    { 10, 2, "Manila Folders (Box of 100)", 15.75m, 5, 15, "OFF-005" },
                    { 11, 3, "Cordless Drill 18V", 79.99m, 24, 8, "WH-001" },
                    { 12, 3, "Safety Gloves (Pair)", 8.50m, 95, 30, "WH-002" },
                    { 13, 3, "Pallet Wrap Roll", 22.00m, 14, 15, "WH-003" },
                    { 14, 3, "Heavy Duty Shelving Unit", 145.00m, 6, 5, "WH-004" },
                    { 15, 3, "Digital Luggage Scale", 17.99m, 40, 10, "WH-005" },
                    { 16, 4, "Ergonomic Office Chair", 210.00m, 9, 6, "FUR-001" },
                    { 17, 4, "Standing Desk (Electric)", 380.00m, 4, 5, "FUR-002" },
                    { 18, 4, "2-Drawer Filing Cabinet", 95.00m, 11, 8, "FUR-003" }
                });

            migrationBuilder.InsertData(
                table: "StockMovements",
                columns: new[] { "Id", "ChangeAmount", "ProductId", "Reason", "Timestamp" },
                values: new object[,]
                {
                    { 1, 100, 1, 0, new DateTime(2026, 7, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, -15, 1, 1, new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 20, 3, 0, new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, -8, 3, 1, new DateTime(2026, 7, 6, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 20, 10, 0, new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, -15, 10, 1, new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, -1, 17, 3, new DateTime(2026, 7, 13, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, -7, 8, 2, new DateTime(2026, 7, 15, 9, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
