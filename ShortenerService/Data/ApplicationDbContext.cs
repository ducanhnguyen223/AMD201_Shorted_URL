using Microsoft.EntityFrameworkCore;
using ShortenerService.Entities;

namespace ShortenerService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ShortenedUrl>()
                .HasIndex(s => s.ShortCode)
                .IsUnique();
        }
    }
}