using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using System.Security.Claims;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IAdvertRepository _advertRepository;

        public FavoritesController(IFavoriteRepository favoriteRepository, IAdvertRepository advertRepository)
        {
            _favoriteRepository = favoriteRepository;
            _advertRepository = advertRepository;
        }
        
        [Authorize]
        [HttpPost("{advertId}")]
        public async Task<IActionResult> ToggleFavorite(int advertId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });

            // İlanın var ve aktif olduğunu kontrol et
            var advert = await _advertRepository.GetByIdAsync(advertId);
            if (advert == null || !advert.IsActive)
                return NotFound(new { Message = "İlan bulunamadı." });

            var existing = await _favoriteRepository.GetFavoriteAsync(userId, advertId);

            if (existing != null)
            {
                // Zaten favoride — çıkar
                _favoriteRepository.Remove(existing);
                await _favoriteRepository.SaveChangesAsync();
                return Ok(new { Message = "İlan favorilerden çıkarıldı.", IsFavorited = false });
            }
            else
            {
                // Favoride değil — ekle
                var favorite = new Favorite
                {
                    UserId = userId,
                    AdvertId = advertId,
                    CreatedAt = DateTime.UtcNow
                };
                await _favoriteRepository.AddAsync(favorite);
                await _favoriteRepository.SaveChangesAsync();
                return Ok(new { Message = "İlan favorilere eklendi.", IsFavorited = true });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });

            var adverts = await _favoriteRepository.GetUserFavoriteAdvertsAsync(userId);

            var result = adverts.Select(a => new
            {
                Id = a.Id,
                Title = a.Title,
                Price = a.Price,
                CategoryName = a.Category?.Name ?? "Kategorisiz",
                UserName = $"{a.User?.FirstName} {a.User?.LastName}",
                ImageUrl = a.Images?.OrderBy(i => i.SortOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl ?? a.Images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImageUrl,
                CreatedAt = a.CreatedAt
            }).ToList();

            return Ok(result);
        }


        [Authorize]
        [HttpGet("ids")]
        public async Task<IActionResult> GetMyFavoriteIds()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });

            var ids = await _favoriteRepository.GetUserFavoriteAdvertIdsAsync(userId);
            return Ok(ids);
        }
    }
}
