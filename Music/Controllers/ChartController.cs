using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.DTOs;
using Music.Services;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/charts")]
    public class ChartsController : ControllerBase
    {
        private readonly MusicDbContext _context;
        private readonly ChartSnapshotService _snapshotService;

        public ChartsController(
            MusicDbContext context,
            ChartSnapshotService snapshotService)
        {
            _context = context;
            _snapshotService = snapshotService;
        }

        // ================= GET CHART =================
        [HttpGet("{type}")]
        public async Task<IActionResult> GetChart(string type)
        {
            if (!new[] { "realtime", "daily", "weekly" }.Contains(type))
                return BadRequest("Invalid chart type");

            var latestSnapshot = await _context.SongRankings
                .Where(r => r.ChartType == type)
                .MaxAsync(r => (DateTime?)r.SnapshotTime);

            if (latestSnapshot == null)
                return Ok(new List<ChartSongDto>());

            var chart = await _context.SongRankings
                .Where(r =>
                    r.ChartType == type &&
                    r.SnapshotTime == latestSnapshot)
                .OrderBy(r => r.Rank)
                .Select(r => new ChartSongDto
                {
                    SongId = r.SongId,
                    Title = r.Song.Title,
                    Artist = r.Song.Artist.Name,
                    CoverUrl = r.Song.CoverUrl,
                    AudioUrl = r.Song.AudioUrl,
                    Duration = r.Song.Duration,
                    ArtistId = r.Song.ArtistId,
                    Rank = r.Rank,
                    PrevRank = r.PrevRank,
                    ViewCount = r.ViewCount
                })
                .ToListAsync();

            return Ok(chart);
        }

        // ================= MANUAL SNAPSHOT =================
        [HttpPost("snapshot/{type}")]
        public async Task<IActionResult> CreateSnapshot(string type)
        {
            await _snapshotService.CreateSnapshotAsync(type);

            return Ok(new
            {
                message = "Snapshot created manually",
                chartType = type,
                time = DateTime.UtcNow
            });
        }
    }
}
