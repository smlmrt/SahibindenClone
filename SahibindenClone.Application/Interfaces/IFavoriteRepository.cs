using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces
{
    public interface IFavoriteRepository
    {
        /// <summary>Kullanıcının belirli bir ilanı favoriye ekleyip eklemediğini kontrol eder.</summary>
        Task<Favorite?> GetFavoriteAsync(int userId, int advertId);

        /// <summary>Favoriye ekler.</summary>
        Task AddAsync(Favorite favorite);

        /// <summary>Favoriden çıkarır.</summary>
        void Remove(Favorite favorite);

        /// <summary>Kullanıcının tüm favori ilanlarını (ilan detaylarıyla birlikte) getirir.</summary>
        Task<List<Advert>> GetUserFavoriteAdvertsAsync(int userId);

        /// <summary>Kullanıcının favori ilan ID'lerini getirir (kalp ikonu için).</summary>
        Task<List<int>> GetUserFavoriteAdvertIdsAsync(int userId);

        Task<int> SaveChangesAsync();
    }
}
