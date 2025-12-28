using System.ComponentModel.DataAnnotations.Schema;

namespace Music.Models
{
    [Table("user_follow_artists")]
    public class UserFollowArtist
    {
        public int UserId { get; set; }
        public int ArtistId { get; set; }

        // Navigation
        public User User { get; set; }
        public Artist Artist { get; set; }
    }
}
