namespace Music.DTOs
{
    public class ChartSongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public string AudioUrl { get; set; }

        public int Duration { get; set; }

        public int ArtistId { get; set; }


        public int Rank { get; set; }
        public int? PrevRank { get; set; }
        public int ViewCount { get; set; }
    }
}
