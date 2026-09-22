using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // kullanıcıyı email ile birlikte getirmek.
        Task<User?> GetUserWithAdvertsAsync(int id);

        // Email'e göre kullanıcı getir
        Task<User?> GetUserByEmailAsync(string email);
    }
}