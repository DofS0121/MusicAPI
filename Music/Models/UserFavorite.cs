using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Music.Models
{
    [Table("user_favorites")] // 🔥 QUAN TRỌNG
    public class UserFavorite
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("song_id")]
        public int SongId { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
        public Song Song { get; set; }
    }
}
