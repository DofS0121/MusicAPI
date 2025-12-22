using System.ComponentModel.DataAnnotations.Schema;

namespace Music.Models
{
    public class AddFavoriteDto
    {
        public int UserId { get; set; }
        public int SongId { get; set; }
    }
}
