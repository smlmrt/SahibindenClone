namespace SahibindenClone.Domain.Entities;

public sealed class SellerReview : BaseEntity
{
    public int PurchaseTransactionId { get; set; }
    public virtual PurchaseTransaction PurchaseTransaction { get; set; } = null!;
    public int ReviewerId { get; set; }
    public virtual User Reviewer { get; set; } = null!;
    public int SellerId { get; set; }
    public virtual User Seller { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
