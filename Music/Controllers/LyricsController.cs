using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/lyrics")]
    public class LyricsController : ControllerBase
    {
        private readonly MusicDbContext _context;

        public LyricsController(MusicDbContext context)
        {
            _context = context;
        }

        // ==========================
        // GET: api/lyrics/song/{songId}
        // ==========================
        [HttpGet("song/{songId}")]
        public IActionResult GetLyricsBySong(int songId)
        {
            var lyrics = _context.SongLyrics
                .Where(l => l.SongId == songId)
                .OrderBy(l => l.TimeSecond)
                .Select(l => new
                {
                    time = l.TimeSecond,
                    text = l.LyricText
                })
                .ToList();

            return Ok(lyrics);
        }

        // ==========================
        // POST: api/lyrics/song/{songId}
        // ==========================
        [HttpPost("song/{songId}")]
        public IActionResult AddLyrics(
            int songId,
            [FromBody] List<SongLyricCreateModel> lyrics)
        {
            if (lyrics == null || lyrics.Count == 0)
                return BadRequest(new { message = "Lyrics empty" });

            var entities = lyrics.Select(l => new SongLyric
            {
                SongId = songId,
                TimeSecond = l.TimeSecond,
                LyricText = l.LyricText
            }).ToList();

            _context.SongLyrics.AddRange(entities);
            _context.SaveChanges();

            return Ok(new { message = "Lyrics added", total = entities.Count });
        }

        // ==========================
        // DELETE: api/lyrics/song/{songId}
        // ==========================
        [HttpDelete("song/{songId}")]
        public IActionResult DeleteLyrics(int songId)
        {
            var lyrics = _context.SongLyrics
                .Where(l => l.SongId == songId)
                .ToList();

            if (!lyrics.Any())
                return NotFound(new { message = "No lyrics found" });

            _context.SongLyrics.RemoveRange(lyrics);
            _context.SaveChanges();

            return Ok(new { message = "Lyrics deleted" });
        }
    }
}
