using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SahibindenClone.Domain.Enums;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Services
{
    public class AdvertExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdvertExpirationWorker> _logger;

        public AdvertExpirationWorker(IServiceProvider serviceProvider, ILogger<AdvertExpirationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("İlan süre kontrolü servisi başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Süresi dolmuş ve hala aktif görünen ilanları bul
                    var expiredAdverts = await context.Adverts
                        .Where(a => a.Status == AdvertStatus.Active && a.ExpirationDate <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    if (expiredAdverts.Count > 0)
                    {
                        foreach (var advert in expiredAdverts)
                        {
                            advert.Status = AdvertStatus.Expired;
                            advert.UpdatedAt = DateTime.UtcNow;
                        }
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("{Count} ilanın süresi dolmuş olarak işaretlendi.", expiredAdverts.Count);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Uygulama kapanıyor, normal çıkış
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "İlan süre kontrolü sırasında hata oluştu. Bir sonraki döngüde tekrar denenecek.");
                }

                // Her 1 saatte bir bu kontrolü yap
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}