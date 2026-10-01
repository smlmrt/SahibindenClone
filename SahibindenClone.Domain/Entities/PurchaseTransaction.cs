using SahibindenClone.Domain.Enums;

namespace SahibindenClone.Domain.Entities;

public sealed class PurchaseTransaction : BaseEntity
{
    public int AdvertId { get; set; }
    public virtual Advert Advert { get; set; } = null!;
    public int BuyerId { get; set; }
    public virtual User Buyer { get; set; } = null!;
    public int SellerId { get; set; }
    public virtual User Seller { get; set; } = null!;
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Requested;
    public DateTime? CompletedAt { get; set; }
    public virtual SellerReview? Review { get; set; }
}
