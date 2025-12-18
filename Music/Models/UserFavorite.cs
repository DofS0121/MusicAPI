using System.Text.Json.Serialization;

namespace Music.Models
{
    public class UserFavorite
    {
        public int UserId { get; set; }
        public int SongId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        public Song Song { get; set; }
    }
}
