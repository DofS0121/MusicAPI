using System.ComponentModel.DataAnnotations;

namespace Music.Models
{
    public class CategoryCreateModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
