using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Services;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Domain.Enums;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Services;

public sealed class PurchaseService(ApplicationDbContext db) : IPurchaseService
{
    public async Task<ServiceResult<int>> RequestPurchaseAsync(int advertId, int buyerId)
    {
        var advert = await db.Adverts.FirstOrDefaultAsync(a => a.Id == advertId && a.IsActive && a.User.IsActive);
        if (advert is null) return ServiceResult<int>.Failure(ServiceError.NotFound, "İlan bulunamadı.");
        if (advert.Status != AdvertStatus.Active) return ServiceResult<int>.Failure(ServiceError.InvalidOperation, "Yalnızca aktif ilanlar için satın alma talebi oluşturulabilir.");
        if (advert.UserId == buyerId) return ServiceResult<int>.Failure(ServiceError.InvalidOperation, "Kendi ilanınız için satın alma talebi oluşturamazsınız.");
        if (await db.PurchaseTransactions.AnyAsync(t => t.AdvertId == advertId && t.BuyerId == buyerId &&
                t.Status is PurchaseStatus.Requested or PurchaseStatus.Accepted))
            return ServiceResult<int>.Failure(ServiceError.Conflict, "Bu ilan için zaten yanıt bekleyen veya kabul edilmiş bir talebiniz var.");

        var transaction = new PurchaseTransaction
        {
            AdvertId = advertId, BuyerId = buyerId, SellerId = advert.UserId,
            Status = PurchaseStatus.Requested, CreatedAt = DateTime.UtcNow
        };
        db.PurchaseTransactions.Add(transaction);
        await db.SaveChangesAsync();
        return ServiceResult<int>.Success(transaction.Id);
    }

    public async Task<IReadOnlyList<PurchaseDto>> GetMyPurchasesAsync(int userId)
    {
        var transactions = await db.PurchaseTransactions.AsNoTracking()
            .Where(t => t.BuyerId == userId || t.SellerId == userId)
            .Include(t => t.Advert).Include(t => t.Buyer).Include(t => t.Seller).Include(t => t.Review)
            .OrderByDescending(t => t.CreatedAt).ToListAsync();
        return transactions.Select(t => new PurchaseDto(t.Id, t.AdvertId, t.Advert.Title,
            t.BuyerId, $"{t.Buyer.FirstName} {t.Buyer.LastName}", t.SellerId, $"{t.Seller.FirstName} {t.Seller.LastName}",
            (int)t.Status, StatusName(t.Status), t.CreatedAt, t.CompletedAt,
            t.Status == PurchaseStatus.Completed && t.BuyerId == userId && t.Review is null, t.Review is not null)).ToList();
    }

    public async Task<ServiceResult<bool>> DecideAsync(int transactionId, int sellerId, bool accept)
    {
        var transaction = await db.PurchaseTransactions.Include(t => t.Advert)
            .FirstOrDefaultAsync(t => t.Id == transactionId);
        if (transaction is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Satın alma talebi bulunamadı.");
        if (transaction.SellerId != sellerId) return ServiceResult<bool>.Failure(ServiceError.Forbidden, "Bu talebi yanıtlama yetkiniz yok.");
        if (transaction.Status != PurchaseStatus.Requested) return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Bu talep daha önce yanıtlanmış.");
        if (accept && (!transaction.Advert.IsActive || transaction.Advert.Status != AdvertStatus.Active))
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "İlan artık satışta olmadığı için talep kabul edilemez.");

        transaction.Status = accept ? PurchaseStatus.Accepted : PurchaseStatus.Rejected;
        transaction.UpdatedAt = DateTime.UtcNow;
        if (accept)
        {
            transaction.Advert.Status = AdvertStatus.Sold;
            transaction.Advert.UpdatedAt = DateTime.UtcNow;
            var otherRequests = await db.PurchaseTransactions.Where(t => t.AdvertId == transaction.AdvertId &&
                t.Id != transaction.Id && t.Status == PurchaseStatus.Requested).ToListAsync();
            foreach (var other in otherRequests)
            {
                other.Status = PurchaseStatus.Rejected;
                other.UpdatedAt = DateTime.UtcNow;
            }
        }
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ConfirmCompletionAsync(int transactionId, int buyerId)
    {
        var transaction = await db.PurchaseTransactions.FirstOrDefaultAsync(t => t.Id == transactionId);
        if (transaction is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Satın alma işlemi bulunamadı.");
        if (transaction.BuyerId != buyerId) return ServiceResult<bool>.Failure(ServiceError.Forbidden, "Bu işlemi onaylama yetkiniz yok.");
        if (transaction.Status != PurchaseStatus.Accepted) return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Yalnızca satıcı tarafından kabul edilmiş işlemler tamamlanabilir.");
        transaction.Status = PurchaseStatus.Completed;
        transaction.CompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> AddReviewAsync(int transactionId, int buyerId, SellerReviewCreateDto dto)
    {
        var transaction = await db.PurchaseTransactions.Include(t => t.Review).FirstOrDefaultAsync(t => t.Id == transactionId);
        if (transaction is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Satın alma işlemi bulunamadı.");
        if (transaction.BuyerId != buyerId) return ServiceResult<bool>.Failure(ServiceError.Forbidden, "Yalnızca alıcı satıcı değerlendirmesi yapabilir.");
        if (transaction.Status != PurchaseStatus.Completed) return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Yorum yalnızca tamamlanmış alışverişten sonra eklenebilir.");
        if (transaction.Review is not null) return ServiceResult<bool>.Failure(ServiceError.Conflict, "Bu işlem için zaten değerlendirme yapılmış.");

        db.SellerReviews.Add(new SellerReview
        {
            PurchaseTransactionId = transaction.Id, ReviewerId = buyerId, SellerId = transaction.SellerId,
            Rating = dto.Rating, Comment = dto.Comment.Trim(), CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<SellerReviewSummaryDto> GetSellerReviewsAsync(int sellerId)
    {
        var reviews = await db.SellerReviews.AsNoTracking().Where(r => r.SellerId == sellerId)
            .Include(r => r.Reviewer).OrderByDescending(r => r.CreatedAt).ToListAsync();
        var result = reviews.Select(r => new SellerReviewDto(r.Id, r.PurchaseTransactionId, r.ReviewerId,
            $"{r.Reviewer.FirstName} {r.Reviewer.LastName}", r.SellerId, r.Rating, r.Comment, r.CreatedAt)).ToList();
        return new SellerReviewSummaryDto(sellerId, reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating), reviews.Count, result);
    }

    private static string StatusName(PurchaseStatus status) => status switch
    {
        PurchaseStatus.Requested => "Satıcı yanıtı bekleniyor",
        PurchaseStatus.Accepted => "Satıcı kabul etti, alıcı onayı bekleniyor",
        PurchaseStatus.Rejected => "Reddedildi",
        _ => "Tamamlandı"
    };
}
