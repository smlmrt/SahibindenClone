using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly IGenericRepository<City> _cityRepository;

        public CitiesController(IGenericRepository<City> cityRepository)
        {
            _cityRepository = cityRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _cityRepository.GetAllAsync();
            var activeCities = cities.Where(c => c.IsActive).OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToList();

            return Ok(activeCities);
        }
    }
}