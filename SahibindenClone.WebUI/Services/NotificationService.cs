using Microsoft.AspNetCore.SignalR;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.WebUI.Hubs;

namespace SahibindenClone.WebUI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IGenericRepository<Notification> _notificationRepository;

        // Sınıf adıyla birebir aynı olmalı: NotificationService
        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            IFavoriteRepository favoriteRepository,
            IGenericRepository<Notification> notificationRepository)
        {
            _hubContext = hubContext;
            _favoriteRepository = favoriteRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task NotifyPriceDropAsync(int advertId, string advertTitle, decimal newPrice)
        {
            var userIds = await _favoriteRepository.GetUsersWhoFavoritedAdvertAsync(advertId);
            if (!userIds.Any()) return;

            string message = $"Favorinizdeki '{advertTitle}' ilanının fiyatı {newPrice:N0} TL'ye düştü!";
            string link = $"/advert-detail.html?id={advertId}";

            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    RelatedLink = link,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);

                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message, link);
            }
            
            await _notificationRepository.SaveChangesAsync();
        }
    }
}