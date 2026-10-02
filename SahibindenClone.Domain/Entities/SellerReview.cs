namespace SahibindenClone.Domain.Entities;

public sealed class SellerReview : BaseEntity
{
    public int PurchaseTransactionId { get; set; }
    public PurchaseTransaction PurchaseTransaction { get; set; } = null!;
    public int ReviewerId { get; set; }
    public User Reviewer { get; set; } = null!;
    public int SellerId { get; set; }
    public User Seller { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
