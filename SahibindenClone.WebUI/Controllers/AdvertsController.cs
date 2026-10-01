using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Application.Services;
using SahibindenClone.Domain.Entities;
using System.Security.Claims;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertsController : ControllerBase
    {
        private readonly IAdvertService _advertService;
        private readonly IWebHostEnvironment _env;
        private readonly IAdvertRepository _advertRepository;

        // ──── İzin verilen dosya türleri ve maksimum boyut ────
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public AdvertsController(IAdvertService advertService, IAdvertRepository advertRepository, IWebHostEnvironment env)
        {
            _advertService = advertService;
            _advertRepository = advertRepository;
            _env = env;
        }

        [Authorize]
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetChangeHistory(int id)
        {
            var advert = await _advertRepository.GetByIdAsync(id);
            if (advert is null || !advert.IsActive) return NotFound("İlan bulunamadı.");
            if (!TryGetUserId(out var userId)) return Unauthorized();
            if (advert.UserId != userId && !User.IsInRole("Admin")) return Forbid();
            return Ok(await _advertService.GetChangeHistoryAsync(id));
        }

        // İlan listesi — Arama, Filtreleme, Sayfalama destekli
        [HttpGet]
        public async Task<IActionResult> GetAdverts(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] int? cityId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? brand,
            [FromQuery] string? model,
            [FromQuery] DateOnly? createdFrom,
            [FromQuery] DateOnly? createdTo,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return Ok(await _advertService.GetAdvertsAsync(search, categoryId, cityId, minPrice, maxPrice,
                brand, model, createdFrom, createdTo, sortBy, page, pageSize));
        }

        // İlan detayı — GET /api/adverts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdvertById(int id)
        {
            var advert = await _advertService.GetByIdAsync(id);

            if (advert == null) return NotFound("İlan bulunamadı.");

            return Ok(new
            {
                advert.Id, advert.Title, advert.Price, advert.Description, advert.Brand, advert.Model, advert.CategoryId, advert.CategoryName,
                advert.CityId, advert.CityName, advert.UserName, advert.UserId, advert.Status, advert.StatusName,
                advert.CreatedAt, advert.Images
            });
        }

        // İlan oluşturma — POST /api/adverts
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAdvert([FromForm] AdvertCreateDto dto, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = GetValidationErrors() });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });

            var advertImages = new List<AdvertImage>();

            if (images != null && images.Count > 0)
            {
                int sortOrder = 0;
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var imageValidation = ValidateImage(image);
                        if (imageValidation != null)
                            return BadRequest(new { Errors = new[] { imageValidation } });

                        var imageUrl = await SaveImageAsync(image);
                        advertImages.Add(new AdvertImage
                        {
                            ImageUrl = imageUrl,
                            IsMain = sortOrder == 0,
                            SortOrder = sortOrder++
                        });
                    }
                }
            }

            var createResult = await _advertService.CreateAsync(dto, userId, advertImages);
            return Ok(new { Message = "İlan başarıyla oluşturuldu!", Id = createResult.Value });
        }

        // İlan güncelleme — PUT /api/adverts/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdvert(int id, [FromForm] AdvertUpdateDto dto, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = GetValidationErrors() });

            if (!TryGetUserId(out var userId)) return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });
            var permissionResult = await _advertService.CanUpdateAsync(id, userId);
            if (!permissionResult.Succeeded) return ToActionResult(permissionResult, string.Empty);
            var advertImages = new List<AdvertImage>();
            if (images != null && images.Count > 0)
            {
                int sortOrder = 0;
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var imageValidation = ValidateImage(image);
                        if (imageValidation != null)
                            return BadRequest(new { Errors = new[] { imageValidation } });

                        var imageUrl = await SaveImageAsync(image);
                        advertImages.Add(new AdvertImage
                        {
                            ImageUrl = imageUrl,
                            IsMain = false,
                            SortOrder = sortOrder++
                        });
                    }
                }
            }

            var updateResult = await _advertService.UpdateAsync(id, dto, userId, advertImages);
            return ToActionResult(updateResult, "İlan başarıyla güncellendi!");
        }

        // Satıldı Olarak İşaretle — PUT /api/adverts/{id}/mark-sold
        [Authorize]
        [HttpPut("{id}/mark-sold")]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });
            var result = await _advertService.MarkAsSoldAsync(id, userId);
            return ToActionResult(result, "İlan başarıyla 'Satıldı' olarak işaretlendi!");
        }

        // İlan silme (soft delete) — DELETE /api/adverts/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdvert(int id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized(new { Message = "Geçersiz kullanıcı bilgisi." });
            var result = await _advertService.DeleteAsync(id, userId);
            return ToActionResult(result, "İlan başarıyla silindi.");
        }

        private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);

        private IActionResult ToActionResult<T>(ServiceResult<T> result, string successMessage)
        {
            if (result.Succeeded) return Ok(new { Message = successMessage });
            var body = new { Message = result.Message };
            return result.Error switch
            {
                ServiceError.NotFound => NotFound(body),
                ServiceError.Forbidden => StatusCode(403, body),
                ServiceError.Unauthorized => Unauthorized(body),
                _ => BadRequest(body)
            };
        }

        // ──── Yardımcı: Görsel dosya validasyonu ────
        private static string? ValidateImage(IFormFile image)
        {
            if (image.Length > MaxFileSize)
            {
                var sizeMB = MaxFileSize / (1024 * 1024);
                return $"Görsel boyutu en fazla {sizeMB}MB olabilir. Yüklenen: {image.Length / (1024.0 * 1024.0):F1}MB";
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return $"Desteklenmeyen dosya türü: {extension}. İzin verilen türler: {string.Join(", ", AllowedExtensions)}";
            }

            if (!AllowedMimeTypes.Contains(image.ContentType.ToLowerInvariant()))
            {
                return $"Desteklenmeyen dosya içerik türü: {image.ContentType}. İzin verilen: {string.Join(", ", AllowedMimeTypes)}";
            }

            return null;
        }

        private List<string> GetValidationErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrEmpty(msg))
                .ToList();
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + uniqueFileName;
        }
    }
}
