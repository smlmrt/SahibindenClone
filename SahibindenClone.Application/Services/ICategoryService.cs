using SahibindenClone.Application.DTOs;

namespace SahibindenClone.Application.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync();
}
