using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllWithSubCategoriesAsync();

        // Admin paneli: Kategoriler (üst kategori adı ve ilan sayısıyla)
        Task<IEnumerable<Category>> GetAllCategoriesWithDetailsAsync();

        // Alt kategorisi var mı kontrolü
        Task<bool> HasActiveSubCategoriesAsync(int parentId);
    }
}
