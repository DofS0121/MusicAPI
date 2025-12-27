using System.Text.Json.Serialization;

namespace Music.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string? CoverUrl { get; set; }

        [JsonIgnore] // ⛔ nếu client không cần xem danh sách bài từ đây
        public List<PlaylistSong> PlaylistSongs { get; set; }
    }
}
