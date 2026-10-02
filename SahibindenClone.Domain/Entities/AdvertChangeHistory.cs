namespace SahibindenClone.Domain.Entities;

public sealed class AdvertChangeHistory : BaseEntity
{
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
    public int ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;
    public string PreviousTitle { get; set; } = string.Empty;
    public string NewTitle { get; set; } = string.Empty;
    public string PreviousDescription { get; set; } = string.Empty;
    public string NewDescription { get; set; } = string.Empty;
    public decimal PreviousPrice { get; set; }
    public decimal NewPrice { get; set; }
}
