using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllWithSubCategoriesAsync();
    }
}
