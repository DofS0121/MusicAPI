using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    public class FavoritesController : ControllerBase
    {
        private readonly MusicDbContext _context;

        public FavoritesController(MusicDbContext context)
        {
            _context = context;
        }

        // ===============================
        // POST: api/favorites
        // Body: { userId, songId }
        // ===============================
        [HttpPost]
        public IActionResult AddFavorite([FromBody] AddFavoriteDto dto)
        {
            // check tồn tại
            if (!_context.Users.Any(u => u.Id == dto.UserId))
                return BadRequest("User not found");

            if (!_context.Songs.Any(s => s.Id == dto.SongId))
                return BadRequest("Song not found");

            // tránh duplicate
            var exists = _context.UserFavorites.Any(f =>
                f.UserId == dto.UserId && f.SongId == dto.SongId);

            if (exists)
                return BadRequest("Already in favorites");

            var fav = new UserFavorite
            {
                UserId = dto.UserId,
                SongId = dto.SongId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFavorites.Add(fav);
            _context.SaveChanges();

            return Ok(new { message = "Added to favorites" });
        }

        // ===============================
        // GET: api/favorites/{userId}
        // ===============================
        [HttpGet("{userId}")]
        public IActionResult GetFavorites(int userId)
        {
            var songs = _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => new
                {
                    id = f.Song.Id,
                    title = f.Song.Title,
                    artist = f.Song.Artist.Name,
                    coverUrl = f.Song.CoverUrl,
                    audioUrl = f.Song.AudioUrl,
                    artistId = f.Song.ArtistId,
                    views = f.Song.Views
                })
                .ToList();

            return Ok(songs);
        }

        // ===============================
        // DELETE: api/favorites
        // Body: { userId, songId }
        // ===============================
        [HttpDelete]
        public IActionResult RemoveFavorite([FromBody] AddFavoriteDto dto)
        {
            var fav = _context.UserFavorites
                .FirstOrDefault(f =>
                    f.UserId == dto.UserId &&
                    f.SongId == dto.SongId
                );

            if (fav == null)
                return NotFound("Favorite not found");

            _context.UserFavorites.Remove(fav);
            _context.SaveChanges();

            return Ok(new { message = "Removed from favorites" });
        }

    }
}
