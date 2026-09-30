using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using System.Security.Claims;


namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IGenericRepository<Notification> _notificationRepository;

        public NotificationsController(IGenericRepository<Notification> notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var notifications = await _notificationRepository.FindAsync(n => n.UserId == userId);

            var result = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.RelatedLink,
                    n.IsRead,
                    n.CreatedAt
                });

            return Ok(result);
        }

        [HttpPut("mark-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            // Okunmamış olanları bul
            var unreadNotifications = await _notificationRepository.FindAsync(n => n.UserId == userId && !n.IsRead);
            
            foreach (var notif in unreadNotifications)
            {
                notif.IsRead = true;
                _notificationRepository.Update(notif);
            }

            await _notificationRepository.SaveChangesAsync();
            return Ok(new { Message = "Tüm bildirimler okundu." });
        }
    }
}