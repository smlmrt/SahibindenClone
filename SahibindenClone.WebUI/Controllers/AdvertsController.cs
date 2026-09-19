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

        [HttpGet]
        public async Task<IActionResult> GetAdverts()
        {
            var adverts = await _advertRepository.GetAdvertsWithDetailsAsync();
            var dtoList = adverts.Select(a => new AdvertListDto
            {
                Id = a.Id, Title = a.Title, Price = a.Price, 
                CategoryName = a.Category?.Name ?? "Kategorisiz", 
                UserName = $"{a.User?.FirstName} {a.User?.LastName}", 
                CreatedAt = a.CreatedAt, ImageUrl = a.ImageUrl
            }).ToList();
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdvertById(int id)
        {
            var adverts = await _advertRepository.GetAdvertsWithDetailsAsync();
            var advert = adverts.FirstOrDefault(a => a.Id == id);

            if (advert == null) return NotFound("İlan bulunamadı.");

            return Ok(new
            {
                Id = advert.Id, Title = advert.Title, Price = advert.Price,
                Description = advert.Description,
                CategoryName = advert.Category?.Name ?? "Kategorisiz",
                UserName = $"{advert.User?.FirstName} {advert.User?.LastName}",
                CreatedAt = advert.CreatedAt, ImageUrl = advert.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdvert([FromForm] AdvertCreateDto dto, IFormFile? image)
        {
            string? imageUrl = null;

            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                imageUrl = "/images/" + uniqueFileName;
            }

            var newAdvert = new Domain.Entities.Advert
            {
                Title = dto.Title, Description = dto.Description, Price = dto.Price,
                CategoryId = dto.CategoryId, UserId = dto.UserId, ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow, IsActive = true
            };

            await _advertRepository.AddAsync(newAdvert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Message = "İlan başarıyla oluşturuldu!" });
        }
    }
}