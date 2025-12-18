using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/artists")]
    public class ArtistsController : ControllerBase
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ArtistsController(MusicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================
        // GET: api/artists
        // ==========================
        [HttpGet]
        public IActionResult GetArtists()
        {
            var artists = _context.Artists
                .AsNoTracking()
                .Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    avatar = a.AvatarUrl,
                    totalSongs = _context.Songs.Count(s => s.ArtistId == a.Id)
                })
                .ToList();

            return Ok(artists);
        }

        // ==========================
        // GET: api/artists/{id}
        // ==========================
        [HttpGet("{id}")]
        public IActionResult GetArtist(int id)
        {
            var artist = _context.Artists
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    avatar = a.AvatarUrl,
                    bio = a.Bio
                })
                .FirstOrDefault();

            if (artist == null)
                return NotFound(new { message = "Artist not found" });

            return Ok(artist);
        }

        // ==========================
        // GET: api/artists/{id}/songs
        // ==========================
        [HttpGet("{id}/songs")]
        public IActionResult GetSongsByArtist(int id)
        {
            if (!_context.Artists.Any(a => a.Id == id))
                return NotFound(new { message = "Artist not found" });

            var songs = _context.Songs
                .AsNoTracking()
                .Where(s => s.ArtistId == id)
                .OrderByDescending(s => s.Views)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    audioUrl = s.AudioUrl,
                    duration = s.Duration,
                    views = s.Views
                })
                .ToList();

            return Ok(songs);
        }

        // ==========================
        // POST: api/artists
        // ==========================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateArtist([FromForm] ArtistCreateUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { message = "Artist name is required" });

            string? avatarUrl = null;

            // ===== Avatar upload =====
            if (model.AvatarFile != null)
            {
                var ext = Path.GetExtension(model.AvatarFile.FileName).ToLower();
                var allowExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowExt.Contains(ext))
                    return BadRequest(new { message = "Avatar không hợp lệ" });

                var fileName = $"artist_{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "avatar_artist",
                    fileName
                );

                using var stream = new FileStream(savePath, FileMode.Create);
                await model.AvatarFile.CopyToAsync(stream);

                avatarUrl = "/avatar_artist/" + fileName;
            }

            var artist = new Artist
            {
                Name = model.Name,
                Bio = model.Bio,
                AvatarUrl = avatarUrl
            };

            _context.Artists.Add(artist);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Artist created",
                artistId = artist.Id,
                avatar = artist.AvatarUrl
            });
        }


        // ==========================
        // PUT: api/artists/{id}
        // ==========================
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateArtist(
            int id,
            [FromForm] ArtistCreateUpdateModel model)
        {
            var artist = _context.Artists.FirstOrDefault(a => a.Id == id);
            if (artist == null)
                return NotFound(new { message = "Artist not found" });

            if (!string.IsNullOrWhiteSpace(model.Name))
                artist.Name = model.Name;

            if (model.Bio != null)
                artist.Bio = model.Bio;

            // ===== Update avatar =====
            if (model.AvatarFile != null)
            {
                var ext = Path.GetExtension(model.AvatarFile.FileName).ToLower();
                var allowExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowExt.Contains(ext))
                    return BadRequest(new { message = "Avatar không hợp lệ" });

                var fileName = $"artist_{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "avatar_artist",
                    fileName
                );

                using var stream = new FileStream(savePath, FileMode.Create);
                await model.AvatarFile.CopyToAsync(stream);

                artist.AvatarUrl = "/avatar_artist/" + fileName;
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Artist updated",
                avatar = artist.AvatarUrl
            });
        }
    }
}
