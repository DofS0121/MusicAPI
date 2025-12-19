using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Music.Models
{
    [Table("categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<SongCategory> SongCategories { get; set; }
    }
}
