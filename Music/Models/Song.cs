using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Music.Models
{
    [Table("songs")]
    public class Song
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        [Column("artist_id")]
        public int ArtistId { get; set; }

        [ForeignKey("ArtistId")]
        public Artist Artist { get; set; }

        [Column("artist_type")]
        public string ArtistType { get; set; }

        [Column("audio_url")]
        public string AudioUrl { get; set; }

        [Column("cover_url")]
        public string CoverUrl { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public int Duration { get; set; }

        public int Views { get; set; }

        // 🔥 MANY TO MANY
        public ICollection<SongCategory> SongCategories { get; set; }

        [JsonIgnore]
        public ICollection<UserFavorite> FavoritedBy { get; set; }
    }
}
