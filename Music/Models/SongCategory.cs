using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Music.Models
{
    [Table("songcategories")]
    public class SongCategory
    {
        public int SongId { get; set; }

        [ForeignKey("SongId")]
        public Song Song { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
