using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Music.Models
{
    [Table("songlyrics")]
    public class SongLyric
    {
        [Key]
        public int Id { get; set; }

        [Column("SongId")]
        public int SongId { get; set; }

        [Column("TimeSecond")]
        public float TimeSecond { get; set; }

        [Column("LyricText")]
        public string LyricText { get; set; }
    }
}
