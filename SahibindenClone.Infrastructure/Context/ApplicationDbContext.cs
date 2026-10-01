using Microsoft.EntityFrameworkCore;
using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Infrastructure.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Advert> Adverts { get; set; }
        public DbSet<AdvertImage> AdvertImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
        public DbSet<SellerReview> SellerReviews { get; set; }
        public DbSet<AdvertChangeHistory> AdvertChangeHistories { get; set; }
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().Property(u => u.Role).HasDefaultValue("User");
            modelBuilder.Entity<Advert>().Property(a => a.Brand).HasMaxLength(100);
            modelBuilder.Entity<Advert>().Property(a => a.Model).HasMaxLength(100);

            modelBuilder.Entity<PurchaseTransaction>().HasOne(t => t.Advert).WithMany().HasForeignKey(t => t.AdvertId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseTransaction>().HasOne(t => t.Buyer).WithMany().HasForeignKey(t => t.BuyerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseTransaction>().HasOne(t => t.Seller).WithMany().HasForeignKey(t => t.SellerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SellerReview>().HasOne(r => r.PurchaseTransaction).WithOne(t => t.Review).HasForeignKey<SellerReview>(r => r.PurchaseTransactionId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SellerReview>().HasIndex(r => new { r.PurchaseTransactionId, r.ReviewerId }).IsUnique();
            modelBuilder.Entity<SellerReview>().HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SellerReview>().HasOne(r => r.Seller).WithMany().HasForeignKey(r => r.SellerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AdvertChangeHistory>().HasOne(h => h.Advert).WithMany().HasForeignKey(h => h.AdvertId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AdvertChangeHistory>().HasOne(h => h.ChangedByUser).WithMany().HasForeignKey(h => h.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AdminAuditLog>().HasOne(l => l.AdminUser).WithMany().HasForeignKey(l => l.AdminUserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Advert>()
                .HasOne(a => a.City)
                .WithMany(c => c.Adverts)
                .HasForeignKey(a => a.CityId)
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
            
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
