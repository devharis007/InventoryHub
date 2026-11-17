using InventoryHub.Data;
using InventoryHub.Models;
using InventoryHub.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Controllers.Api
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly InventoryContext _context;

        public CategoriesApiController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products.Count
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, ProductCount = c.Products.Count })
                .FirstOrDefaultAsync();

            if (category == null) return NotFound();
            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Category name is required.");

            var category = new Category { Name = dto.Name.Trim() };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryDto { Id = category.Id, Name = category.Name, ProductCount = 0 };
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, result);
        }

        // DELETE: api/categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();

            if (category.Products.Any())
                return BadRequest("Cannot delete a category that still has products assigned to it.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
