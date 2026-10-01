using SahibindenClone.Application.DTOs;

namespace SahibindenClone.Application.Services;

public interface IPurchaseService
{
    Task<ServiceResult<int>> RequestPurchaseAsync(int advertId, int buyerId);
    Task<IReadOnlyList<PurchaseDto>> GetMyPurchasesAsync(int userId);
    Task<ServiceResult<bool>> DecideAsync(int transactionId, int sellerId, bool accept);
    Task<ServiceResult<bool>> ConfirmCompletionAsync(int transactionId, int buyerId);
    Task<ServiceResult<bool>> AddReviewAsync(int transactionId, int buyerId, SellerReviewCreateDto dto);
    Task<SellerReviewSummaryDto> GetSellerReviewsAsync(int sellerId);
}
