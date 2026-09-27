using Microsoft.EntityFrameworkCore;
using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Infrastructure.Context
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (!context.Users.Any())
            {
                var users = new User[]
                {
                    new User { FirstName = "Ahmet", LastName = "Yılmaz", Email = "sahibindenclone.test", PasswordHash="hash_placeholder", IsActive = true, CreatedAt = DateTime.UtcNow}
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category { Name = "Emlak", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Vasıta", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Category { Name = "İkinci El ve Sıfır Alışveriş", IsActive = true, CreatedAt = DateTime.UtcNow }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            var defaultCities = new[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya" };
            bool anyNewCity = false;
            foreach (var cityName in defaultCities)
            {
                if (!context.Cities.Any(c => c.Name == cityName))
                {
                    context.Cities.Add(new City { Name = cityName, IsActive = true, CreatedAt = DateTime.UtcNow });
                    anyNewCity = true;
                }
            }

            if (anyNewCity)
            {
                context.SaveChanges();
            }
        }
    }
}