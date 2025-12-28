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
                .Include(a => a.Songs)
                .FirstOrDefault(a => a.Id == id);

            if (artist == null)
                return NotFound(new { message = "Artist not found" });

            var dto = new ArtistDTO
            {
                Id = artist.Id,
                Name = artist.Name,
                AvatarUrl = artist.AvatarUrl,
                Bio = artist.Bio,
                TotalSongs = artist.Songs.Count
            };

            return Ok(dto);
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
                .Where(s => s.ArtistId == id)
                .Select(s => new {
                    id = s.Id,
                    title = s.Title,
                    artist = s.Artist.Name, // thêm tên ca sĩ để hiển thị
                    artistId = s.ArtistId,
                    audioUrl = s.AudioUrl,
                    coverUrl = s.CoverUrl,
                    duration = s.Duration,
                    bio = s.Artist.Bio,
                    avatarUrl = s.Artist.AvatarUrl,
                    views = s.Views
                }).ToList();

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

        // ==========================
        // POST: api/artists/follow
        // ==========================
        [HttpPost("follow")]
        public IActionResult FollowArtist(int userId, int artistId)
        {
            if (!_context.Artists.Any(a => a.Id == artistId))
                return NotFound(new { message = "Artist not found" });

            bool existed = _context.UserFollowArtists
                .Any(f => f.UserId == userId && f.ArtistId == artistId);

            if (existed) return BadRequest(new { message = "Đã quan tâm nghệ sĩ này" });

            _context.UserFollowArtists.Add(new UserFollowArtist
            {
                UserId = userId,
                ArtistId = artistId
            });
            _context.SaveChanges();

            return Ok(new { message = "Đã quan tâm nghệ sĩ" });
        }

        // ==========================
        // DELETE: api/artists/unfollow
        // ==========================
        [HttpDelete("unfollow")]
        public IActionResult UnfollowArtist(int userId, int artistId)
        {
            var follow = _context.UserFollowArtists
                .FirstOrDefault(f => f.UserId == userId && f.ArtistId == artistId);

            if (follow == null) return BadRequest(new { message = "Chưa quan tâm nghệ sĩ này" });

            _context.UserFollowArtists.Remove(follow);
            _context.SaveChanges();

            return Ok(new { message = "Đã bỏ quan tâm" });
        }

        // GET: api/artists/{id}/isFollowed?userId=1
        [HttpGet("{id}/isFollowed")]
        public IActionResult IsFollowed(int id, int userId)
        {
            bool isFollowed = _context.UserFollowArtists
                .Any(f => f.UserId == userId && f.ArtistId == id);

            return Ok(new { isFollowed });
        }

        // GET: api/artists/search?query=ade
        [HttpGet("search")]
        public IActionResult SearchArtists(string query)
        {
            query = query.ToLower();
            var artists = _context.Artists
                .Where(a => a.Name.ToLower().Contains(query))
                .Select(a => new {
                    id = a.Id,
                    name = a.Name,
                    avatar = a.AvatarUrl,
                    totalSongs = _context.Songs.Count(s => s.ArtistId == a.Id)
                })
                .ToList();

            return Ok(artists);
        }

        // ==========================
        // GET: api/artists/followed?userId=1
        // ==========================
        [HttpGet("followed")]
        public IActionResult GetFollowedArtists(int userId)
        {
            var artists = _context.UserFollowArtists
                .Where(f => f.UserId == userId)
                .Select(f => new
                {
                    id = f.Artist.Id,
                    name = f.Artist.Name,
                    avatar = f.Artist.AvatarUrl,
                    bio = f.Artist.Bio,
                    totalSongs = _context.Songs.Count(s => s.ArtistId == f.ArtistId)
                })
                .ToList();

            return Ok(artists);
        }
    }
}
