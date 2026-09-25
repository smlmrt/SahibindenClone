using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly ApplicationDbContext _context;

        public FavoriteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Favorite?> GetFavoriteAsync(int userId, int advertId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.AdvertId == advertId);
        }

        public async Task AddAsync(Favorite favorite)
        {
            await _context.Favorites.AddAsync(favorite);
        }

        public void Remove(Favorite favorite)
        {
            _context.Favorites.Remove(favorite);
        }

        public async Task<List<Advert>> GetUserFavoriteAdvertsAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId && f.Advert.IsActive)
                .Include(f => f.Advert)
                    .ThenInclude(a => a.Category)
                .Include(f => f.Advert)
                    .ThenInclude(a => a.User)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Advert)
                .ToListAsync();
        }

        public async Task<List<int>> GetUserFavoriteAdvertIdsAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.AdvertId)
                .ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
