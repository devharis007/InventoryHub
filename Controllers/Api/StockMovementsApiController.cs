using InventoryHub.Data;
using InventoryHub.Models;
using InventoryHub.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Controllers.Api
{
    [ApiController]
    [Route("api/stockmovements")]
    public class StockMovementsApiController : ControllerBase
    {
        private readonly InventoryContext _context;

        public StockMovementsApiController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/stockmovements?productId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetMovements([FromQuery] int? productId)
        {
            var query = _context.StockMovements.Include(sm => sm.Product).AsQueryable();

            if (productId.HasValue)
                query = query.Where(sm => sm.ProductId == productId.Value);

            var movements = await query
                .OrderByDescending(sm => sm.Timestamp)
                .Take(200)
                .Select(sm => new StockMovementDto
                {
                    Id = sm.Id,
                    ProductId = sm.ProductId,
                    ProductName = sm.Product != null ? sm.Product.Name : string.Empty,
                    ChangeAmount = sm.ChangeAmount,
                    Reason = sm.Reason.ToString(),
                    Timestamp = sm.Timestamp
                })
                .ToListAsync();

            return Ok(movements);
        }

        // POST: api/stockmovements  (adjusts stock and records the movement)
        [HttpPost]
        public async Task<ActionResult<StockMovementDto>> CreateMovement(StockAdjustmentDto dto)
        {
            if (dto.ChangeAmount == 0)
                return BadRequest("Change amount cannot be zero.");

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound("Product not found.");

            var newQuantity = product.QuantityInStock + dto.ChangeAmount;
            if (newQuantity < 0)
                return BadRequest("Stock adjustment would result in negative inventory.");

            product.QuantityInStock = newQuantity;

            var movement = new StockMovement
            {
                ProductId = product.Id,
                ChangeAmount = dto.ChangeAmount,
                Reason = dto.Reason,
                Timestamp = DateTime.UtcNow
            };
            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync();

            return Ok(new StockMovementDto
            {
                Id = movement.Id,
                ProductId = movement.ProductId,
                ProductName = product.Name,
                ChangeAmount = movement.ChangeAmount,
                Reason = movement.Reason.ToString(),
                Timestamp = movement.Timestamp
            });
        }
    }
}
