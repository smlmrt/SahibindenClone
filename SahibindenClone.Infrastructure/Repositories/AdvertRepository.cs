using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Repositories
{
    public class AdvertRepository : GenericRepository<Advert>, IAdvertRepository
    {
        public AdvertRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Advert>> GetAdvertsWithDetailsAsync()
        {
            return await _context.Adverts
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.User)
                .Include(a => a.Images)
                .Where(a => a.IsActive)
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
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        }

        public async Task<IEnumerable<Advert>> GetAdvertsByCategoryIdAsync(int categoryId)
        {
            return await _context.Adverts
                .Include(a => a.Images)
                .Where(a => a.CategoryId == categoryId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Advert> Items, int TotalCount)> GetFilteredAdvertsAsync(
            string? search,
            int? categoryId,
            int? cityId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize)
        {
            var query = _context.Adverts
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.User)
                .Include(a => a.Images)
                .Where(a => a.IsActive)
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

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
    }
}