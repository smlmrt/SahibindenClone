using SahibindenClone.Domain.Enums;

namespace SahibindenClone.Domain.Entities;

public sealed class PurchaseTransaction : BaseEntity
{
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
    public int BuyerId { get; set; }
    public User Buyer { get; set; } = null!;
    public int SellerId { get; set; }
    public User Seller { get; set; } = null!;
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Requested;
    public DateTime? CompletedAt { get; set; }
    public SellerReview? Review { get; set; }
}
