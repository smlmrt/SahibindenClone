using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Repositories
{
    public class AdvertRepository : GenericRepository<Advert>, IAdvertRepository
    {
        public AdvertRepository(ApplicationDbContext context) : base(context) { }

        public async Task AddChangeHistoryAsync(AdvertChangeHistory history) => await _context.AdvertChangeHistories.AddAsync(history);

        public async Task<IReadOnlyList<AdvertChangeHistory>> GetChangeHistoryAsync(int advertId) => await _context.AdvertChangeHistories
            .AsNoTracking().Include(h => h.ChangedByUser).Where(h => h.AdvertId == advertId)
            .OrderByDescending(h => h.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Advert>> GetAdvertsWithDetailsAsync()
        {
            return await _context.Adverts
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.User)
                .Include(a => a.Images)
                .Where(a => a.IsActive && a.User.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Advert?> GetAdvertWithDetailsByIdAsync(int id)
        {
            return await _context.Adverts
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.User)
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive && a.User.IsActive);
        }

        public async Task<IEnumerable<Advert>> GetAdvertsByCategoryIdAsync(int categoryId)
        {
            return await _context.Adverts
                .Include(a => a.Images)
                .Where(a => a.CategoryId == categoryId && a.IsActive && a.User.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Advert> Items, int TotalCount)> GetFilteredAdvertsAsync(
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
            int pageSize)
        {
            var query = _context.Adverts
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.User)
                .Include(a => a.Images)
                .Where(a => a.IsActive && a.User.IsActive && a.Status == Domain.Enums.AdvertStatus.Active)
                .AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchLower) ||
                    a.Description.ToLower().Contains(searchLower));
            }

            // Kategori filtresi
            if (categoryId.HasValue) query = query.Where(a => a.CategoryId == categoryId.Value);

            // ŞEHİR FİLTRESİ (YENİ EKLENDİ)
            if (cityId.HasValue) query = query.Where(a => a.CityId == cityId.Value);

            // Minimum ve Maksimum fiyat filtreleri
            if (minPrice.HasValue) query = query.Where(a => a.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(a => a.Price <= maxPrice.Value);
            if (!string.IsNullOrWhiteSpace(brand)) query = query.Where(a => a.Brand != null && a.Brand.ToLower().Contains(brand.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(model)) query = query.Where(a => a.Model != null && a.Model.ToLower().Contains(model.Trim().ToLower()));
            if (createdFrom.HasValue) query = query.Where(a => a.CreatedAt >= createdFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            if (createdTo.HasValue) query = query.Where(a => a.CreatedAt < createdTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

            var totalCount = await query.CountAsync();
            query = sortBy?.ToLowerInvariant() switch
            {
                "oldest" => query.OrderBy(a => a.CreatedAt),
                "price-asc" => query.OrderBy(a => a.Price).ThenByDescending(a => a.CreatedAt),
                "price-desc" => query.OrderByDescending(a => a.Price).ThenByDescending(a => a.CreatedAt),
                _ => query.OrderByDescending(a => a.CreatedAt)
            };
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
    }
}
