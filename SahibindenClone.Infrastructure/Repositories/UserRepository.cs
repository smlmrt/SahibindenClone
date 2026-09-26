using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) {}

        public async Task<User?> GetUserWithAdvertsAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Adverts.Where(a => a.IsActive))
                .ThenInclude(a => a.Category)
                .Include(u => u.Adverts)
                .ThenInclude(a => a.Images)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
        }
    }
}