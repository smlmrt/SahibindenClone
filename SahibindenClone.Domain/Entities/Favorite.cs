namespace SahibindenClone.Domain.Entities
{
    public class Favorite
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Favoriyi ekleyen kullanıcı
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Favoriye eklenen ilan
        public int AdvertId { get; set; }
        public virtual Advert Advert { get; set; } = null!;
    }
}
