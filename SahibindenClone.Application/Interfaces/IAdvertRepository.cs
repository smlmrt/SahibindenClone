using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces
{
    public interface IAdvertRepository : IGenericRepository<Advert>
    {
        Task<IEnumerable<Advert>> GetAdvertsWithDetailsAsync();
        Task<IEnumerable<Advert>> GetAdvertsByCategoryIdAsync(int categoryId);
    }
}