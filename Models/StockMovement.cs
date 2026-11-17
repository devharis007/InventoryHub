using System;

namespace InventoryHub.Models
{
    public enum StockMovementReason
    {
        Restock,
        Sale,
        Adjustment,
        Damaged
    }

    public class StockMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int ChangeAmount { get; set; }

        public StockMovementReason Reason { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
