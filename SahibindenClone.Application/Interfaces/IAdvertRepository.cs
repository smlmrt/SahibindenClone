using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces
{
    public interface IAdvertRepository : IGenericRepository<Advert>
    {
        Task<IEnumerable<Advert>> GetAdvertsWithDetailsAsync();
        Task<Advert?> GetAdvertWithDetailsByIdAsync(int id);
        Task<IEnumerable<Advert>> GetAdvertsByCategoryIdAsync(int categoryId);
        Task AddChangeHistoryAsync(AdvertChangeHistory history);
        Task<IReadOnlyList<AdvertChangeHistory>> GetChangeHistoryAsync(int advertId);

        /// <summary>
        /// Filtreleme, arama ve sayfalama destekli ilan listesi
        /// </summary>
        Task<(IEnumerable<Advert> Items, int TotalCount)> GetFilteredAdvertsAsync(
            string? search,
            int? categoryId,
            int? cityId,
            decimal? minPrice,
            decimal? maxPrice,
            string? brand,
            string? model,
            DateOnly? createdFrom,
            DateOnly? createdTo,
            string? sortBy,
            int page,
            int pageSize);
    }
}
