using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ================= ADD FAVORITE =================
        [HttpPost]
        public IActionResult AddFavorite([FromBody] AddFavoriteDto dto)
        {
            // 1️⃣ Validate basic
            if (dto.UserId <= 0 || dto.SongId <= 0)
                return BadRequest("Invalid UserId or SongId");

            // 2️⃣ Check tồn tại User
            var userExists = _context.Users.Any(u => u.Id == dto.UserId);
            if (!userExists)
                return BadRequest("User not found");

            // 3️⃣ Check tồn tại Song
            var songExists = _context.Songs.Any(s => s.Id == dto.SongId);
            if (!songExists)
                return BadRequest("Song not found");

            // 4️⃣ Check đã yêu thích chưa
            var exists = _context.UserFavorites
                .Any(f => f.UserId == dto.UserId && f.SongId == dto.SongId);

            if (exists)
                return BadRequest("Already favorited");

            // 5️⃣ Add
            var favorite = new UserFavorite
            {
                UserId = dto.UserId,
                SongId = dto.SongId
            };

            _context.UserFavorites.Add(favorite);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Added to favorites"
            });
        }

        // ================= GET FAVORITES =================
        [HttpGet("{userId}")]
        public IActionResult GetFavorites(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid userId");

            var songs = _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Song)
                .ThenInclude(s => s.Artist)
                .Select(f => new
                {
                    id = f.Song.Id,
                    title = f.Song.Title ?? "",
                    artist = f.Song.Artist != null
                        ? f.Song.Artist.Name
                        : "Unknown",
                    cover_url = f.Song.CoverUrl ?? "",
                    audio_url = f.Song.AudioUrl ?? "",
                    duration = f.Song.Duration,
                    views = f.Song.Views
                })
                .OrderByDescending(s => s.id)
                .ToList();

            return Ok(songs);
        }


        // ================= REMOVE FAVORITE (OPTIONAL) =================
        [HttpDelete]
        public IActionResult RemoveFavorite([FromBody] AddFavoriteDto dto)
        {
            var favorite = _context.UserFavorites
                .FirstOrDefault(f => f.UserId == dto.UserId && f.SongId == dto.SongId);

            if (favorite == null)
                return NotFound("Favorite not found");

            _context.UserFavorites.Remove(favorite);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Removed from favorites"
            });
        }
    }
}
