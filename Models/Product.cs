using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace InventoryHub.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(40)]
        public string SKU { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int QuantityInStock { get; set; }

        public int ReorderThreshold { get; set; }

        public List<StockMovement> StockMovements { get; set; } = new();
    }
}
