using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Application.DTOs;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertsController : ControllerBase
    {
        private readonly IAdvertRepository _advertRepository;
        private readonly IWebHostEnvironment _env;

        public AdvertsController(IAdvertRepository advertRepository, IWebHostEnvironment env)
        {
            _advertRepository = advertRepository;
            _env = env;
        }

        /// <summary>
        /// İlan listesi — Arama, Filtreleme, Sayfalama destekli
        /// GET /api/adverts?search=araba&categoryId=2&minPrice=100&maxPrice=5000&page=1&pageSize=20
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAdverts(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Geçerlilik kontrolleri
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var (adverts, totalCount) = await _advertRepository.GetFilteredAdvertsAsync(
                search, categoryId, minPrice, maxPrice, page, pageSize);

            var dtoList = adverts.Select(a => new AdvertListDto
            {
                Id = a.Id,
                Title = a.Title,
                Price = a.Price,
                CategoryName = a.Category?.Name ?? "Kategorisiz",
                UserName = $"{a.User?.FirstName} {a.User?.LastName}",
                CreatedAt = a.CreatedAt,
                ImageUrl = a.ImageUrl
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

        /// <summary>
        /// İlan detayı — GET /api/adverts/{id}
        /// </summary>
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
                UserName = $"{advert.User?.FirstName} {advert.User?.LastName}",
                CreatedAt = advert.CreatedAt,
                ImageUrl = advert.ImageUrl
            });
        }

        /// <summary>
        /// İlan oluşturma — POST /api/adverts
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAdvert([FromForm] AdvertCreateDto dto, IFormFile? image)
        {
            string? imageUrl = null;

            if (image != null && image.Length > 0)
            {
                imageUrl = await SaveImageAsync(image);
            }

            var newAdvert = new Domain.Entities.Advert
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                UserId = dto.UserId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _advertRepository.AddAsync(newAdvert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla oluşturuldu!", Id = newAdvert.Id });
        }

        /// <summary>
        /// İlan güncelleme — PUT /api/adverts/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdvert(int id, [FromForm] AdvertUpdateDto dto, IFormFile? image)
        {
            var advert = await _advertRepository.GetByIdAsync(id);
            if (advert == null || !advert.IsActive)
                return NotFound("İlan bulunamadı.");

            // Alanları güncelle
            advert.Title = dto.Title;
            advert.Description = dto.Description;
            advert.Price = dto.Price;
            advert.CategoryId = dto.CategoryId;
            advert.UpdatedAt = DateTime.UtcNow;

            // Yeni görsel yüklendiyse güncelle
            if (image != null && image.Length > 0)
            {
                // Eski görseli sil (varsa)
                if (!string.IsNullOrEmpty(advert.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, advert.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                advert.ImageUrl = await SaveImageAsync(image);
            }

            _advertRepository.Update(advert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla güncellendi!" });
        }

        /// <summary>
        /// İlan silme (soft delete) — DELETE /api/adverts/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdvert(int id)
        {
            var advert = await _advertRepository.GetByIdAsync(id);
            if (advert == null || !advert.IsActive)
                return NotFound("İlan bulunamadı.");

            // Soft delete — veritabanından silmez, IsActive = false yapar
            advert.IsActive = false;
            advert.UpdatedAt = DateTime.UtcNow;

            _advertRepository.Update(advert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla silindi." });
        }

        // ──── Yardımcı metod: Görsel kaydetme ────
        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + uniqueFileName;
        }
    }
}