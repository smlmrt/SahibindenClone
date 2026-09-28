using SahibindenClone.Domain.Enums;

namespace SahibindenClone.Domain.Entities
{
    public class Advert : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public AdvertStatus Status { get;set; } = AdvertStatus.Active;
        public DateTime ExpirationDate { get; set; }

        // İlanın Sahibi
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // İlanın Kategorisi
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        // İlna Konumu 
        public int CityId { get; set; }
        public virtual City City { get; set; } = null!;

        // İlan Görselleri
        public virtual ICollection<AdvertImage> Images { get; set; } = new List<AdvertImage>();

        // İlanı favoriye ekleyen kullanıcılar
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}