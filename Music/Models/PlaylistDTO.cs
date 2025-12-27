namespace Music.Models
{
    public class PlaylistDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? CoverUrl { get; set; }
        public int TotalSongs { get; set; }
    }
}
