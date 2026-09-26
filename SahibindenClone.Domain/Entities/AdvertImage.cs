namespace SahibindenClone.Domain.Entities
{
    public class AdvertImage : BaseEntity
    {
        public int AdvertId { get; set; }
        public virtual Advert Advert { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
    }
}
