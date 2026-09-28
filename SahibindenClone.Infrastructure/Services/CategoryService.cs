using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Application.Services;

namespace SahibindenClone.Infrastructure.Services;

public sealed class CategoryService(ICategoryRepository categories) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        var all = await categories.GetAllWithSubCategoriesAsync();
        return all.Where(c => c.ParentId is null).Select(c => new CategoryDto
        {
            Id = c.Id, Name = c.Name, ParentId = c.ParentId,
            SubCategories = c.SubCategories.Where(s => s.IsActive).Select(s => new CategoryDto { Id = s.Id, Name = s.Name, ParentId = s.ParentId }).ToList()
        }).ToList();
    }
}
