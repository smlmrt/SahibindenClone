using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SahibindenClone.Domain.Enums;
using SahibindenClone.Infrastructure.Context;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SahibindenClone.Infrastructure.Services
{
    public class AdvertExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AdvertExpirationWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Süresi dolmuş ve hala aktif görünen ilanları bul
                    var expiredAdverts = await context.Adverts
                        .Where(a => a.Status == AdvertStatus.Active && a.ExpirationDate <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    if (expiredAdverts.Any())
                    {
                        foreach (var advert in expiredAdverts)
                        {
                            advert.Status = AdvertStatus.Expired;
                            advert.UpdatedAt = DateTime.UtcNow;
                        }
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                // Her 1 saatte bir bu kontrolü yap
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}