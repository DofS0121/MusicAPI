using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly MusicDbContext _context;

        public FavoritesController(MusicDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddFavorite([FromBody] UserFavorite fav)
        {
            _context.UserFavorites.Add(fav);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("{userId}")]
        public IActionResult GetFavorites(int userId)
        {
            var data = _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Song)
                .ToList();

            return Ok(data);
        }
    }
}
