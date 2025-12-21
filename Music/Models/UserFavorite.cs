using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Music.Models
{
    [Table("user_favorites")]
    public class UserFavorite
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("song_id")]
        public int SongId { get; set; }

        [JsonIgnore]
        [BindNever]   // ⭐ QUAN TRỌNG
        public User? User { get; set; }

        [JsonIgnore]
        [BindNever]   // ⭐ QUAN TRỌNG
        public Song? Song { get; set; }
    }
}
