using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

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

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Adverts = user.Adverts.Select(a => new AdvertListDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Price = a.Price,
                    CategoryName = a.Category?.Name ?? "Kategorisiz",
                    UserName = $"{user.FirstName} {user.LastName}",
                    ImageUrl = a.ImageUrl,
                    CreatedAt = a.CreatedAt
                }).OrderByDescending(a => a.CreatedAt).ToList()
            };

            return Ok(profileDto);
        }

        /// PUT: api/users/{id}/profile
        /// Kullanıcı profil bilgilerini (ve istenirse şifresini) günceller
        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UserProfileUpdateDto dto)
        {
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

            // Basit şifre değişikliği mantığı (Gerçek bir projede hash'lenmeli)
            if (!string.IsNullOrEmpty(dto.CurrentPassword) && !string.IsNullOrEmpty(dto.NewPassword))
            {
                if (user.PasswordHash != dto.CurrentPassword)
                {
                    return BadRequest(new { Message = "Mevcut şifreniz hatalı." });
                }
                user.PasswordHash = dto.NewPassword;
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