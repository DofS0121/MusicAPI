using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/playlist")]
    public class PlaylistController : Controller
    {
        private readonly MusicDbContext _context;

        public PlaylistController(MusicDbContext context, IConfiguration config)
        {
            _context = context;
        }

        private PlaylistDTO ToDTO(Playlist p)
        {
            return new PlaylistDTO
            {
                Id = p.Id,
                Name = p.Name,
                CoverUrl = p.CoverUrl,
                TotalSongs = _context.PlaylistSongs.Count(x => x.PlaylistId == p.Id)
            };
        }

        [HttpGet("list/{userId}")]
        public IActionResult GetPlaylistsByUser(int userId)
        {
            var playlists = _context.Playlists
                .Where(p => p.UserId == userId)
                .Select(p => new PlaylistDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    CoverUrl = p.CoverUrl,
                    TotalSongs = _context.PlaylistSongs.Count(x => x.PlaylistId == p.Id)
                })
                .ToList();

            return Ok(new { playlists });
        }

        [HttpGet("{playlistId}")]
        public IActionResult GetPlaylistDetail(int playlistId)
        {
            var playlist = _context.Playlists
                .FirstOrDefault(p => p.Id == playlistId);

            if (playlist == null)
                return NotFound("Playlist không tồn tại");

            var songs = _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Join(_context.Songs,
                    ps => ps.SongId,
                    s => s.Id,
                    (ps, s) => new
                    {
                        s.Id,
                        s.Title,
                        s.CoverUrl,
                        s.AudioUrl,
                        s.Duration,
                        s.ArtistId,
                        ArtistName = s.Artist.Name,
                        views = s.Views
                    })
                .ToList();

            return Ok(new
            {
                playlist = new PlaylistDTO
                {
                    Id = playlist.Id,
                    Name = playlist.Name,
                    CoverUrl = playlist.CoverUrl,
                    TotalSongs = songs.Count
                },
                songs
            });
        }


        [HttpPost("create")]
        public IActionResult CreatePlaylist(int userId, string name)
        {
            var playlist = new Playlist
            {
                UserId = userId,
                Name = name
            };

            _context.Playlists.Add(playlist);
            _context.SaveChanges();

            var result = ToDTO(playlist);

            return Ok(new
            {
                message = "Tạo playlist thành công",
                playlist = result
            });
        }



        [HttpPost("add-song")]
        public IActionResult AddSongToPlaylist(int playlistId, int songId)
        {
            var playlist = _context.Playlists.FirstOrDefault(p => p.Id == playlistId);
            if (playlist == null) return NotFound("Playlist không tồn tại");

            var song = _context.Songs.FirstOrDefault(s => s.Id == songId);
            if (song == null) return NotFound("Bài hát không tồn tại");

            bool existed = _context.PlaylistSongs
                .Any(p => p.PlaylistId == playlistId && p.SongId == songId);

            if (existed) return BadRequest("Bài hát đã có trong playlist");

            // ➕ Thêm bài vào playlist
            _context.PlaylistSongs.Add(new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId
            });

            // 🖼 Nếu chưa có ảnh bìa, lấy ảnh cover bài đầu tiên
            if (string.IsNullOrEmpty(playlist.CoverUrl))
            {
                playlist.CoverUrl = song.CoverUrl;
                _context.Playlists.Update(playlist);
            }

            _context.SaveChanges();

            var result = ToDTO(playlist);

            return Ok(new
            {
                message = "Thêm bài hát thành công",
                playlist = result
            });
        }

        [HttpDelete("delete/{playlistId}")]
        public IActionResult DeletePlaylist(int playlistId)
        {
            var playlist = _context.Playlists.FirstOrDefault(p => p.Id == playlistId);
            if (playlist == null) return NotFound("Playlist không tồn tại");

            // Xóa các bài thuộc playlist
            var songsInPlaylist = _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId);
            _context.PlaylistSongs.RemoveRange(songsInPlaylist);

            // Xóa playlist
            _context.Playlists.Remove(playlist);
            _context.SaveChanges();

            return Ok(new { message = "Đã xóa playlist thành công" });
        }

        [HttpDelete("remove-song")]
        public IActionResult RemoveSong(int playlistId, int songId)
        {
            var playlist = _context.Playlists.Include(p => p.PlaylistSongs)
                            .FirstOrDefault(p => p.Id == playlistId);
            if (playlist == null) return NotFound("Playlist không tồn tại");

            var ps = _context.PlaylistSongs
                    .FirstOrDefault(x => x.PlaylistId == playlistId && x.SongId == songId);
            if (ps == null) return BadRequest("Bài không có trong playlist");

            // 🗑 Remove
            _context.PlaylistSongs.Remove(ps);
            _context.SaveChanges();

            // 📌 Cập nhật cover nếu cần
            var firstSong = _context.PlaylistSongs
                    .Where(x => x.PlaylistId == playlistId)
                    .Select(x => x.Song.CoverUrl)
                    .FirstOrDefault();

            playlist.CoverUrl = firstSong; // null nếu không còn bài
            _context.SaveChanges();

            return Ok(new { message = "Đã xoá", cover = playlist.CoverUrl });
        }

        [HttpPut("rename")]
        public IActionResult RenamePlaylist(int playlistId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return BadRequest("Tên mới không hợp lệ");

            var playlist = _context.Playlists.FirstOrDefault(p => p.Id == playlistId);
            if (playlist == null)
                return NotFound("Playlist không tồn tại");

            playlist.Name = newName.Trim();
            _context.SaveChanges();

            return Ok(new
            {
                message = "Đổi tên playlist thành công",
                playlist = new
                {
                    playlist.Id,
                    playlist.Name,
                    playlist.CoverUrl,
                    TotalSongs = _context.PlaylistSongs.Count(x => x.PlaylistId == playlistId)
                }
            });
        }


    }
}
