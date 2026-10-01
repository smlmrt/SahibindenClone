using System.ComponentModel.DataAnnotations;

namespace SahibindenClone.Application.DTOs;

public sealed class PurchaseRequestDto
{
    [Range(1, int.MaxValue)]
    public int AdvertId { get; set; }
}

public sealed class PurchaseDecisionDto
{
    public bool Accept { get; set; }
}

public sealed class SellerReviewCreateDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, StringLength(1000, MinimumLength = 3)]
    public string Comment { get; set; } = string.Empty;
}

public sealed record PurchaseDto(int Id, int AdvertId, string AdvertTitle, int BuyerId, string BuyerName,
    int SellerId, string SellerName, int Status, string StatusName, DateTime CreatedAt, DateTime? CompletedAt,
    bool CanReview, bool HasReviewed);

public sealed record SellerReviewDto(int Id, int PurchaseTransactionId, int ReviewerId, string ReviewerName,
    int SellerId, int Rating, string Comment, DateTime CreatedAt);

public sealed record SellerReviewSummaryDto(int SellerId, double AverageRating, int ReviewCount, IReadOnlyList<SellerReviewDto> Reviews);

public sealed record AdvertChangeHistoryDto(int Id, int AdvertId, int ChangedByUserId, string ChangedByName,
    string PreviousTitle, string NewTitle, string PreviousDescription, string NewDescription,
    decimal PreviousPrice, decimal NewPrice, DateTime CreatedAt);
