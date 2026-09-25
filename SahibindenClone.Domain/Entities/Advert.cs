namespace SahibindenClone.Domain.Entities
{
    public class Advert : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // İlanın Sahibi
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string? ImageUrl { get; set; }

        // İlanın Kategorisi
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        // İlanı favoriye ekleyen kullanıcılar
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}