using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Application.Services;
using System.Security.Claims;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPurchaseService _purchaseService;

        public UsersController(IUserRepository userRepository, IPurchaseService purchaseService)
        {
            _userRepository = userRepository;
            _purchaseService = purchaseService;
        }

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetSellerReviews(int id) => Ok(await _purchaseService.GetSellerReviewsAsync(id));

        /// GET: api/users/{id}/profile
        /// Kullanıcı profilini ve aktif ilanlarını getirir
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            var user = await _userRepository.GetUserWithAdvertsAsync(id);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Email yalnızca profil sahibine gösterilir (kişisel veri koruması)
            var isOwner = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentUserId) && currentUserId == id;

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = isOwner ? user.Email : null,
                CreatedAt = user.CreatedAt,
                Adverts = user.Adverts.Select(a => new AdvertListDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Price = a.Price,
                    CategoryName = a.Category?.Name ?? "Kategorisiz",
                    UserName = $"{user.FirstName} {user.LastName}",
                    ImageUrl = a.Images?.OrderBy(i => i.SortOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl ?? a.Images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImageUrl,
                    CreatedAt = a.CreatedAt
                }).OrderByDescending(a => a.CreatedAt).ToList()
            };

            return Ok(profileDto);
        }

        /// PUT: api/users/{id}/profile
        /// Kullanıcı profil bilgilerini (ve istenirse şifresini) günceller
        [Authorize]
        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UserProfileUpdateDto dto)
        {
            // Token sahibi yalnızca kendi profilini güncelleyebilir
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var tokenUserId))
                return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });
            if (tokenUserId != id)
                return StatusCode(403, new { Message = "Yalnızca kendi profilinizi güncelleyebilirsiniz." });
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || !user.IsActive)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Email değişikliği varsa çakışma kontrolü
            if (user.Email != dto.Email)
            {
                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(new { Message = "Bu email adresi zaten kullanımda." });
                }
            }

            // Şifre değişikliği — BCrypt ile doğrulama ve hash'leme
            if (!string.IsNullOrEmpty(dto.CurrentPassword) && !string.IsNullOrEmpty(dto.NewPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { Message = "Mevcut şifreniz hatalı." });
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            // Bilgileri güncelle
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { Message = "Profil başarıyla güncellendi." });
        }
    }
}
