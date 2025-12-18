using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Music.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; } = "default.png";

        public string Role { get; set; }
        public string Status { get; set; }

        [JsonIgnore]
        public ICollection<UserFavorite> Favorites { get; set; }
    }
}
