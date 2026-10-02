using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces;

/// <summary>
/// Tüm repository'leri tek bir noktadan yöneten ve atomik işlemler (transaction) sağlayan Unit of Work arayüzü.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IAdvertRepository Adverts { get; }
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IFavoriteRepository Favorites { get; }
    IGenericRepository<Notification> Notifications { get; }
    IAdminAuditLogRepository AuditLogs { get; }
    IGenericRepository<PurchaseTransaction> Purchases { get; }
    IGenericRepository<SellerReview> SellerReviews { get; }

    /// <summary>Tüm değişiklikleri veritabanına kaydeder.</summary>
    Task<int> SaveChangesAsync();

    /// <summary>Yeni bir veritabanı transaction'ı başlatır.</summary>
    Task BeginTransactionAsync();

    /// <summary>Mevcut transaction'ı onaylar (commit).</summary>
    Task CommitAsync();

    /// <summary>Mevcut transaction'ı geri alır (rollback).</summary>
    Task RollbackAsync();
}
