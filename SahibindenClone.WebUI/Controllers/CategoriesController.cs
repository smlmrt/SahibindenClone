using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryRepository.GetAllWithSubCategoriesAsync();

            // Sadece üst kategorileri al (ParentId == null), alt kategorileri içiçe döndür
            var rootCategories = categories
                .Where(c => c.ParentId == null)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    SubCategories = c.SubCategories
                        .Where(sc => sc.IsActive)
                        .Select(sc => new CategoryDto
                        {
                            Id = sc.Id,
                            Name = sc.Name,
                            ParentId = sc.ParentId
                        }).ToList()
                }).ToList();

            return Ok(rootCategories);
        }
    }
}
