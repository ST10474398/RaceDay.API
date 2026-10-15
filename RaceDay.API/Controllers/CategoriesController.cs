using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        // Public / Any logged-in user can view
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Category not found.");
            return Ok(category);
        }

        // POST: api/Categories
        // ONLY Admin can create categories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] Category newCategory)
        {
            newCategory.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Category created successfully!", CategoryID = newCategory.CategoryID });
        }

        // PUT: api/Categories/5
        // ONLY Admin can update categories
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updatedCategory)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null) return NotFound("Category not found.");

            existingCategory.Name = updatedCategory.Name;
            existingCategory.Description = updatedCategory.Description;
            existingCategory.AgeGroup = updatedCategory.AgeGroup;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Category updated successfully!" });
        }

        // DELETE: api/Categories/5
        // ONLY Admin can delete categories
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound("Category not found.");

            // Per Part 1 plan: 409 Conflict if category has events
            if (category.Events.Any())
            {
                return Conflict("Cannot delete a category that has events assigned to it.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Category deleted successfully!" });
        }
    }
}