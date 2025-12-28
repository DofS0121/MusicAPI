namespace Music.Models
{
    public class ArtistDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public int TotalSongs { get; set; }
    }
}
