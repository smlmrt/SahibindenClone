using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Enums;
using System.Security.Claims;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertsController : ControllerBase
    {
        private readonly IAdvertRepository _advertRepository;
        private readonly IWebHostEnvironment _env;

        // ──── İzin verilen dosya türleri ve maksimum boyut ────
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public AdvertsController(IAdvertRepository advertRepository, IWebHostEnvironment env)
        {
            _advertRepository = advertRepository;
            _env = env;
        }

        // İlan listesi — Arama, Filtreleme, Sayfalama destekli
        [HttpGet]
        public async Task<IActionResult> GetAdverts(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] int? cityId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var (adverts, totalCount) = await _advertRepository.GetFilteredAdvertsAsync(
                search, categoryId, cityId, minPrice, maxPrice, page, pageSize);

            var dtoList = adverts.Select(a => new AdvertListDto
            {
                Id = a.Id,
                Title = a.Title,
                Price = a.Price,
                CategoryName = a.Category?.Name ?? "Kategorisiz",
                CityName = a.City?.Name ?? "Belirtilmemiş",
                UserName = $"{a.User?.FirstName} {a.User?.LastName}",
                Status = (int)a.Status,
                StatusName = a.Status == AdvertStatus.Active ? "Aktif" :
                             a.Status == AdvertStatus.Sold ? "Satıldı" :
                             a.Status == AdvertStatus.Expired ? "Süresi Doldu" : "Onay Bekliyor",
                CreatedAt = a.CreatedAt,
                ImageUrl = a.Images?.OrderBy(i => i.SortOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl 
                           ?? a.Images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImageUrl
            }).ToList();

            var result = new PaginatedResultDto<AdvertListDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

        // İlan detayı — GET /api/adverts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdvertById(int id)
        {
            var advert = await _advertRepository.GetAdvertWithDetailsByIdAsync(id);

            if (advert == null) return NotFound("İlan bulunamadı.");

            return Ok(new
            {
                Id = advert.Id,
                Title = advert.Title,
                Price = advert.Price,
                Description = advert.Description,
                CategoryId = advert.CategoryId,
                CategoryName = advert.Category?.Name ?? "Kategorisiz",
                CityId = advert.CityId,
                CityName = advert.City?.Name ?? "Belirtilmemiş",
                UserName = $"{advert.User?.FirstName} {advert.User?.LastName}",
                UserId = advert.UserId, // Frontend yetki kontrolü
                Status = (int)advert.Status,
                StatusName = advert.Status == AdvertStatus.Active ? "Aktif" :
                             advert.Status == AdvertStatus.Sold ? "Satıldı" :
                             advert.Status == AdvertStatus.Expired ? "Süresi Doldu" : "Onay Bekliyor",
                CreatedAt = advert.CreatedAt,
                Images = advert.Images?.OrderBy(i => i.SortOrder).Select(i => new {
                    i.Id,
                    i.ImageUrl,
                    i.IsMain,
                    i.SortOrder
                }).ToList()
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

            var newAdvert = new Domain.Entities.Advert
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                CityId = dto.CityId,
                UserId = userId,
                Status = AdvertStatus.Active, // İlan başlarken Aktif
                ExpirationDate = DateTime.UtcNow.AddDays(30), // 30 gün süre
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Images = new List<Domain.Entities.AdvertImage>()
            };

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
                        newAdvert.Images.Add(new Domain.Entities.AdvertImage
                        {
                            ImageUrl = imageUrl,
                            IsMain = sortOrder == 0,
                            SortOrder = sortOrder++
                        });
                    }
                }
            }

            await _advertRepository.AddAsync(newAdvert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla oluşturuldu!", Id = newAdvert.Id });
        }

        // İlan güncelleme — PUT /api/adverts/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdvert(int id, [FromForm] AdvertUpdateDto dto, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = GetValidationErrors() });

            var advert = await _advertRepository.GetAdvertWithDetailsByIdAsync(id);
            if (advert == null || !advert.IsActive)
                return NotFound(new { Message = "İlan bulunamadı." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (advert.UserId.ToString() != userId)
                return StatusCode(403, new { Message = "Bu ilanı düzenleme yetkiniz yok." });

            advert.Title = dto.Title;
            advert.Description = dto.Description;
            advert.Price = dto.Price;
            advert.CategoryId = dto.CategoryId;
            advert.CityId = dto.CityId;
            advert.UpdatedAt = DateTime.UtcNow;

            if (images != null && images.Count > 0)
            {
                int sortOrder = advert.Images?.Count ?? 0;
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var imageValidation = ValidateImage(image);
                        if (imageValidation != null)
                            return BadRequest(new { Errors = new[] { imageValidation } });

                        var imageUrl = await SaveImageAsync(image);
                        if (advert.Images == null) advert.Images = new List<Domain.Entities.AdvertImage>();
                        advert.Images.Add(new Domain.Entities.AdvertImage
                        {
                            ImageUrl = imageUrl,
                            IsMain = sortOrder == 0 && advert.Images.Count == 0,
                            SortOrder = sortOrder++
                        });
                    }
                }
            }

            _advertRepository.Update(advert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla güncellendi!" });
        }

        // Satıldı Olarak İşaretle — PUT /api/adverts/{id}/mark-sold
        [Authorize]
        [HttpPut("{id}/mark-sold")]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            var advert = await _advertRepository.GetByIdAsync(id);
            if (advert == null || !advert.IsActive)
                return NotFound(new { Message = "İlan bulunamadı." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (advert.UserId.ToString() != userId)
                return StatusCode(403, new { Message = "Bu işlemi yapma yetkiniz yok." });

            if (advert.Status == AdvertStatus.Sold)
                return BadRequest(new { Message = "İlan zaten satıldı olarak işaretlenmiş." });

            advert.Status = AdvertStatus.Sold;
            advert.UpdatedAt = DateTime.UtcNow;

            _advertRepository.Update(advert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla 'Satıldı' olarak işaretlendi!" });
        }

        // İlan silme (soft delete) — DELETE /api/adverts/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdvert(int id)
        {
            var advert = await _advertRepository.GetByIdAsync(id);
            if (advert == null || !advert.IsActive)
                return NotFound(new { Message = "İlan bulunamadı." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (advert.UserId.ToString() != userId)
                return StatusCode(403, new { Message = "Bu ilanı silme yetkiniz yok." });

            advert.IsActive = false;
            advert.UpdatedAt = DateTime.UtcNow;

            _advertRepository.Update(advert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla silindi." });
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