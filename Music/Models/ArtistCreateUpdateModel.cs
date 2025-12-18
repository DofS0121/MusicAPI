using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Music.Models
{
    public class ArtistCreateUpdateModel
    {
        [Required]
        public string Name { get; set; }

        public string? Bio { get; set; }

        // upload file (optional)
        public IFormFile? AvatarFile { get; set; }
    }
}
