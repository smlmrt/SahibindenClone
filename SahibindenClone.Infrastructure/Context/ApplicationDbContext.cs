using Microsoft.EntityFrameworkCore;
using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Infrastructure.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Advert> Adverts { get; set; }
        public DbSet<AdvertImage> AdvertImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Aynı kullanıcı aynı ilanı birden fazla kez favoriye eklemesin
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.AdvertId })
                .IsUnique();

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Advert)
                .WithMany(a => a.Favorites)
                .HasForeignKey(f => f.AdvertId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdvertImage>()
                .HasOne(ai => ai.Advert)
                .WithMany(a => a.Images)
                .HasForeignKey(ai => ai.AdvertId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}