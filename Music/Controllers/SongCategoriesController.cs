using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/song-categories")]
    public class SongCategoriesController : ControllerBase
    {
        private readonly MusicDbContext _context;

        public SongCategoriesController(MusicDbContext context)
        {
            _context = context;
        }

        // =================================================
        // GET: api/song-categories/song/{songId}
        // =================================================
        [HttpGet("song/{songId}")]
        public IActionResult GetCategoriesBySong(int songId)
        {
            var categories = _context.SongCategories
                .Where(sc => sc.SongId == songId)
                .Select(sc => new
                {
                    id = sc.Category.Id,
                    name = sc.Category.Name
                })
                .ToList();

            return Ok(categories);
        }

        // =================================================
        // POST: api/song-categories
        // Body: { "songId": 1, "categoryId": 2 }
        // =================================================
        [HttpPost]
        public IActionResult AddCategory(int songId, int categoryId)
        {
            if (!_context.Songs.Any(s => s.Id == songId))
                return BadRequest("Song not found");

            if (!_context.Categories.Any(c => c.Id == categoryId))
                return BadRequest("Category not found");

            if (_context.SongCategories.Any(sc =>
                sc.SongId == songId && sc.CategoryId == categoryId))
            {
                return BadRequest("Already exists");
            }

            var entity = new SongCategory
            {
                SongId = songId,
                CategoryId = categoryId
            };

            _context.SongCategories.Add(entity);
            _context.SaveChanges();

            return Ok(new { message = "Category added to song" });
        }

        // =================================================
        // DELETE: api/song-categories
        // =================================================
        [HttpDelete]
        public IActionResult RemoveCategory(int songId, int categoryId)
        {
            var entity = _context.SongCategories
                .FirstOrDefault(sc =>
                    sc.SongId == songId &&
                    sc.CategoryId == categoryId);

            if (entity == null)
                return NotFound();

            _context.SongCategories.Remove(entity);
            _context.SaveChanges();

            return Ok(new { message = "Category removed from song" });
        }
    }
}
