using System.Text.Json.Serialization;

namespace Music.Models
{
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }
        public int SongId { get; set; }

        [JsonIgnore]              // ⛔ chặn vòng lặp
        public Playlist Playlist { get; set; }
        public Song Song { get; set; }
    }
}
