using InventoryHub.Data;
using InventoryHub.Models;
using InventoryHub.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly InventoryContext _context;

        public ProductsApiController(InventoryContext context)
        {
            _context = context;
        }

        private static ProductDto ToDto(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            Price = p.Price,
            QuantityInStock = p.QuantityInStock,
            ReorderThreshold = p.ReorderThreshold,
            IsLowStock = p.QuantityInStock < p.ReorderThreshold
        };

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(products.Select(ToDto));
        }

        // GET: api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return Ok(ToDto(product));
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductCreateDto dto)
        {
            var validationError = await ValidateAsync(dto, null);
            if (validationError != null) return BadRequest(validationError);

            var product = new Product
            {
                Name = dto.Name.Trim(),
                SKU = dto.SKU.Trim(),
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                QuantityInStock = dto.QuantityInStock,
                ReorderThreshold = dto.ReorderThreshold
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Record the initial stock as a Restock movement, if any
            if (product.QuantityInStock > 0)
            {
                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = product.Id,
                    ChangeAmount = product.QuantityInStock,
                    Reason = StockMovementReason.Restock,
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ToDto(product));
        }

        // PUT: api/products/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto dto)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var validationError = await ValidateAsync(dto, id);
            if (validationError != null) return BadRequest(validationError);

            product.Name = dto.Name.Trim();
            product.SKU = dto.SKU.Trim();
            product.CategoryId = dto.CategoryId;
            product.Price = dto.Price;
            product.ReorderThreshold = dto.ReorderThreshold;
            // Quantity changes go through the stock-adjustment endpoint, not a direct edit,
            // so the StockMovement history stays authoritative.

            await _context.SaveChangesAsync();
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            return Ok(ToDto(product));
        }

        // DELETE: api/products/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string?> ValidateAsync(ProductCreateDto dto, int? existingId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return "Product name is required.";
            if (string.IsNullOrWhiteSpace(dto.SKU)) return "SKU is required.";
            if (dto.Price < 0) return "Price cannot be negative.";
            if (dto.ReorderThreshold < 0) return "Reorder threshold cannot be negative.";

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) return "Selected category does not exist.";

            var skuTaken = await _context.Products
                .AnyAsync(p => p.SKU == dto.SKU.Trim() && p.Id != (existingId ?? -1));
            if (skuTaken) return "A product with this SKU already exists.";

            return null;
        }
    }
}
