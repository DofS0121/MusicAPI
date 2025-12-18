using Microsoft.EntityFrameworkCore;
using Music.Models;

namespace Music.Data
{
    public class MusicDbContext : DbContext
    {
        public MusicDbContext(DbContextOptions<MusicDbContext> options)
            : base(options) { }

        public DbSet<Song> Songs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<SongLyric> SongLyrics { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set charset cho MySQL
            modelBuilder.UseCollation("utf8mb4_unicode_ci");
            modelBuilder.HasCharSet("utf8mb4");

            // UserFavorite composite key
            modelBuilder.Entity<UserFavorite>()
                .HasKey(uf => new { uf.UserId, uf.SongId });

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Song)
                .WithMany(s => s.FavoritedBy)
                .HasForeignKey(uf => uf.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
