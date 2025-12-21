using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Music.Models
{
    [Table("artists")]
    public class Artist
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Column("avatar_url")]
        public string AvatarUrl { get; set; }

        public string Bio { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Song> Songs { get; set; }
    }
}
