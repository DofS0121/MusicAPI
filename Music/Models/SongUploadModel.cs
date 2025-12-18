using Microsoft.AspNetCore.Http;

namespace Music.Models
{
    public class SongUploadModel
    {
        public string Title { get; set; }

        public int ArtistId { get; set; }

        public string ArtistType { get; set; } // solo | group

        public int Duration { get; set; } // seconds

        public IFormFile AudioFile { get; set; }
        public IFormFile CoverFile { get; set; }
    }
}
