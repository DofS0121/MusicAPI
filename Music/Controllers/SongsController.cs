using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/songs")]
    public class SongsController : ControllerBase
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SongsController(MusicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =================================================
        // GET: api/songs
        // =================================================
        [HttpGet]
        public IActionResult GetAllSongs()
        {
            var songs = _context.Songs
                .Include(s => s.Artist)
                .OrderByDescending(s => s.Id)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    artist = s.Artist.Name,
                    artistId = s.ArtistId,
                    artistAvatar = s.Artist.AvatarUrl,
                    audioUrl = s.AudioUrl,
                    coverUrl = s.CoverUrl,
                    duration = s.Duration,
                    views = s.Views
                })
                .ToList();

            return Ok(songs);
        }

        // =================================================
        // GET: api/songs/{id}
        // (Song Detail – cho Player Page)
        // =================================================
        [HttpGet("{id}")]
        public IActionResult GetSongDetail(int id)
        {
            var song = _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    audioUrl = s.AudioUrl,
                    coverUrl = s.CoverUrl,
                    duration = s.Duration,
                    views = s.Views,
                    artist = new
                    {
                        id = s.Artist.Id,
                        name = s.Artist.Name,
                        avatar = s.Artist.AvatarUrl,
                        bio = s.Artist.Bio
                    }
                })
                .FirstOrDefault();

            if (song == null)
                return NotFound(new { message = "Song not found" });

            return Ok(song);
        }

        // =================================================
        // GET: api/songs/artist/{artistId}
        // (Swipe trái – bài hát của ca sĩ)
        // =================================================
        [HttpGet("artist/{artistId}")]
        public IActionResult GetSongsByArtist(int artistId)
        {
            var songs = _context.Songs
                .Where(s => s.ArtistId == artistId)
                .OrderByDescending(s => s.Id)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    audioUrl = s.AudioUrl,
                    coverUrl = s.CoverUrl,
                    duration = s.Duration
                })
                .ToList();

            return Ok(songs);
        }

        // =================================================
        // POST: api/songs/upload
        // =================================================
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] SongUploadModel model)
        {
            if (model.AudioFile == null || model.CoverFile == null)
                return BadRequest(new { message = "Thiếu file audio hoặc cover" });

            // Check artist
            var artist = _context.Artists.FirstOrDefault(a => a.Id == model.ArtistId);
            if (artist == null)
                return BadRequest(new { message = "Artist không tồn tại" });

            // ===== Validate =====
            var audioExt = Path.GetExtension(model.AudioFile.FileName).ToLower();
            if (audioExt != ".mp3")
                return BadRequest(new { message = "Chỉ hỗ trợ mp3" });

            var coverExt = Path.GetExtension(model.CoverFile.FileName).ToLower();
            var allowCover = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowCover.Contains(coverExt))
                return BadRequest(new { message = "Cover không hợp lệ" });

            // ===== Folder =====
            var audioDir = Path.Combine(_env.WebRootPath, "audio");
            var coverDir = Path.Combine(_env.WebRootPath, "covers");
            Directory.CreateDirectory(audioDir);
            Directory.CreateDirectory(coverDir);

            var audioName = $"{Guid.NewGuid()}{audioExt}";
            var coverName = $"{Guid.NewGuid()}{coverExt}";

            using (var s = new FileStream(Path.Combine(audioDir, audioName), FileMode.Create))
                await model.AudioFile.CopyToAsync(s);

            using (var s = new FileStream(Path.Combine(coverDir, coverName), FileMode.Create))
                await model.CoverFile.CopyToAsync(s);

            var song = new Song
            {
                Title = model.Title,
                ArtistId = model.ArtistId,
                ArtistType = model.ArtistType,
                Duration = model.Duration,
                AudioUrl = "/audio/" + audioName,
                CoverUrl = "/covers/" + coverName,
                Views = 0
            };

            _context.Songs.Add(song);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Upload thành công",
                songId = song.Id
            });
        }

        // POST: api/songs/{id}/view
        [HttpPost("{id}/view")]
        public IActionResult IncreaseView(int id)
        {
            var song = _context.Songs.FirstOrDefault(s => s.Id == id);
            if (song == null)
                return NotFound();

            song.Views += 1;
            _context.SaveChanges();

            return Ok(new { views = song.Views });
        }

        // GET: api/songs/search?q=abc
        [HttpGet("search")]
        public IActionResult Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new List<object>());

            var songs = _context.Songs
                .Include(s => s.Artist)
                .Where(s =>
                    s.Title.Contains(q) ||
                    s.Artist.Name.Contains(q))
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    artist = s.Artist.Name,
                    coverUrl = s.CoverUrl,
                    audioUrl = s.AudioUrl
                })
                .Take(20)
                .ToList();

            return Ok(songs);
        }

    }
}
