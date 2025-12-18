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

        // ===== FK → Artist =====
        [Column("artist_id")]
        public int ArtistId { get; set; }

        [ForeignKey("ArtistId")]
        public Artist Artist { get; set; }

        [Column("artist_type")]
        public string ArtistType { get; set; } // solo | group

        [Column("audio_url")]
        public string AudioUrl { get; set; }

        [Column("cover_url")]
        public string CoverUrl { get; set; }

        public int Duration { get; set; }

        public int Views { get; set; }

        [JsonIgnore]
        public ICollection<UserFavorite> FavoritedBy { get; set; }
    }
}
