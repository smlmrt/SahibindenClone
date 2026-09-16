using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Infrastructure.Context
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Categories.Any())
            {
                return;
            }

            var users = new User[]
            {
                new User { FirstName = "Ahmet", LastName = "Yılmaz", Email = "sahibindenclone.test", PasswordHash="hash_placeholder", IsActive = true, CreatedAt = DateTime.UtcNow}
            };

            context.Users.AddRange(users);
            context.SaveChanges();

            var categories = new Category[]
            {
                new Category { Name = "Emlak", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Vasıta", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "İkinci El ve Sıfır Alışveriş", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();
        }
    }
}