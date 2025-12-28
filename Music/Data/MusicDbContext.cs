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

        public DbSet<Category> Categories { get; set; }

        public DbSet<SongCategory> SongCategories { get; set; }

        public DbSet<SongRanking> SongRankings { get; set; }

        public DbSet<Playlist> Playlists { get; set; }

        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        public DbSet<UserFollowArtist> UserFollowArtists { get; set; }

        public DbSet<PasswordOtp> PasswordOtps { get; set; }



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

            modelBuilder.Entity<SongCategory>()
                .HasKey(sc => new { sc.SongId, sc.CategoryId });

            modelBuilder.Entity<SongCategory>()
                .HasOne(sc => sc.Song)
                .WithMany(s => s.SongCategories)
                .HasForeignKey(sc => sc.SongId);

            modelBuilder.Entity<SongCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SongCategories)
                .HasForeignKey(sc => sc.CategoryId);

            modelBuilder.Entity<PlaylistSong>()
                .HasKey(ps => new { ps.PlaylistId, ps.SongId });

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs)
                .HasForeignKey(ps => ps.PlaylistId);

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Song)
                .WithMany()
                .HasForeignKey(ps => ps.SongId);

            modelBuilder.Entity<UserFollowArtist>()
              .HasKey(f => new { f.UserId, f.ArtistId });

            modelBuilder.Entity<UserFollowArtist>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<UserFollowArtist>()
                .HasOne(f => f.Artist)
                .WithMany()
                .HasForeignKey(f => f.ArtistId);

            modelBuilder.Entity<PasswordOtp>().ToTable("password_otps");
        }
    }
}
