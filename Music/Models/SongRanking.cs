using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Music.Models
{
    [Table("song_rankings")]
    public class SongRanking
    {
        [Key]
        public int Id { get; set; }

        [Column("song_id")]
        public int SongId { get; set; }

        [Column("chart_type")]
        public string ChartType { get; set; } = null!; // realtime | daily | weekly

        public int Rank { get; set; }

        [Column("prev_rank")]
        public int? PrevRank { get; set; }

        [Column("view_count")]
        public int ViewCount { get; set; }

        [Column("snapshot_time")]
        public DateTime SnapshotTime { get; set; }

        public Song Song { get; set; } = null!;
    }
}
