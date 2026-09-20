using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces
{
    public interface IAdvertRepository : IGenericRepository<Advert>
    {
        Task<IEnumerable<Advert>> GetAdvertsWithDetailsAsync();
        Task<Advert?> GetAdvertWithDetailsByIdAsync(int id);
        Task<IEnumerable<Advert>> GetAdvertsByCategoryIdAsync(int categoryId);

        /// <summary>
        /// Filtreleme, arama ve sayfalama destekli ilan listesi
        /// </summary>
        Task<(IEnumerable<Advert> Items, int TotalCount)> GetFilteredAdvertsAsync(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize);
    }
}