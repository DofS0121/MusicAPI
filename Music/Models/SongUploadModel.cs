using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Music.Models
{
    public class SongUploadModel
    {
        public string Title { get; set; }

        public int ArtistId { get; set; }

        public string ArtistType { get; set; } // solo | group

        public int Duration { get; set; } // seconds

        // 🔥 NEW
        public DateTime? ReleaseDate { get; set; }

        // 🔥 MULTI CATEGORY
        // gửi dạng: CategoryIds=1&CategoryIds=2
        public List<int> CategoryIds { get; set; } = new();

        public IFormFile AudioFile { get; set; }
        public IFormFile CoverFile { get; set; }
    }
}
