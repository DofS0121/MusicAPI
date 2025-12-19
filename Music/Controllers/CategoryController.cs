using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly MusicDbContext _context;

        public CategoriesController(MusicDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/categories
        // =========================
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                })
                .ToList();

            return Ok(categories);
        }

        // =========================
        // POST: api/categories
        // =========================
        [HttpPost]
        public IActionResult Create([FromBody] CategoryCreateModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = _context.Categories
                .Any(c => c.Name.ToLower() == model.Name.ToLower());

            if (exists)
                return BadRequest(new { message = "Category đã tồn tại" });

            var category = new Category
            {
                Name = model.Name
            };

            _context.Categories.Add(category);
            _context.SaveChanges();

            return Ok(new
            {
                id = category.Id,
                name = category.Name
            });
        }

        // =========================
        // DELETE: api/categories/{id}
        // =========================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return Ok(new { message = "Deleted" });
        }
    }
}
