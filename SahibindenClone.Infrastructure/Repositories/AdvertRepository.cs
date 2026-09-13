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
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Advert>> GetAdvertsByCategoryIdAsync(int categoryId)
        {
            return await _context.Adverts
                .Where(a => a.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}