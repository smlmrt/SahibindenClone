using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetAllWithSubCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithDetailsAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.Parent)
                .Include(c => c.Adverts)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> HasActiveSubCategoriesAsync(int parentId)
        {
            return await _context.Categories
                .AnyAsync(c => c.ParentId == parentId && c.IsActive);
        }
    }
}
