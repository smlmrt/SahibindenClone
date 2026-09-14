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

        public AdvertsController(IAdvertRepository advertRepository)
        {
            _advertRepository = advertRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAdverts()
        {
            var adverts = await _advertRepository.GetAdvertsWithDetailsAsync();
            var dtoList = adverts.Select(a => new AdvertListDto
            {
                Id = a.Id,
                Title = a.Title,
                Price = a.Price,
                CategoryName = a.Category?.Name ?? "Kategorisiz",
                CreatedAt = a.CreatedAt
            }).ToList();

            return Ok(dtoList); // JSON formatında veri döndürür.
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdvert([FromBody] AdvertCreateDto dto)
        {
            var newAdvert = new Domain.Entities.Advert
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _advertRepository.AddAsync(newAdvert);
            await _advertRepository.SaveChangesAsync();

            return Ok(new { Messahe = "İlan başarıyla oluşturuldu!", AdvertId = newAdvert.Id});
        }
    }
}