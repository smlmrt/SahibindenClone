using Microsoft.EntityFrameworkCore.Storage;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Repositories;

/// <summary>
/// Tüm repository'leri aynı DbContext üzerinden koordine eden Unit of Work implementasyonu.
/// Atomik işlemler ve paylaşımlı SaveChanges desteği sağlar.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IAdvertRepository? _adverts;
    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private IFavoriteRepository? _favorites;
    private IGenericRepository<Notification>? _notifications;
    private IAdminAuditLogRepository? _auditLogs;
    private IGenericRepository<PurchaseTransaction>? _purchases;
    private IGenericRepository<SellerReview>? _sellerReviews;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IAdvertRepository Adverts => _adverts ??= new AdvertRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IFavoriteRepository Favorites => _favorites ??= new FavoriteRepository(_context);
    public IGenericRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(_context);
    public IAdminAuditLogRepository AuditLogs => _auditLogs ??= new AdminAuditLogRepository(_context);
    public IGenericRepository<PurchaseTransaction> Purchases => _purchases ??= new GenericRepository<PurchaseTransaction>(_context);
    public IGenericRepository<SellerReview> SellerReviews => _sellerReviews ??= new GenericRepository<SellerReview>(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
