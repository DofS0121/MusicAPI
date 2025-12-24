using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.Models;

namespace Music.Services
{
    public class ChartSnapshotService
    {
        private readonly MusicDbContext _context;

        public ChartSnapshotService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task CreateSnapshotAsync(string type)
        {
            if (!new[] { "realtime", "daily", "weekly" }.Contains(type))
                throw new ArgumentException("Invalid chart type");

            var now = DateTime.UtcNow;

            var songs = await _context.Songs
                .OrderByDescending(s => s.Views)
                .Take(20)
                .ToListAsync();

            for (int i = 0; i < songs.Count; i++)
            {
                var song = songs[i];

                var prev = await _context.SongRankings
                    .Where(r => r.SongId == song.Id && r.ChartType == type)
                    .OrderByDescending(r => r.SnapshotTime)
                    .FirstOrDefaultAsync();

                _context.SongRankings.Add(new SongRanking
                {
                    SongId = song.Id,
                    ChartType = type,
                    Rank = i + 1,
                    PrevRank = prev?.Rank,
                    ViewCount = song.Views,
                    SnapshotTime = now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
