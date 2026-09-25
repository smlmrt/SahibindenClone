namespace SahibindenClone.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Bir kullanıcının birden fazla ilanı olabilir
        public virtual ICollection<Advert> Adverts { get; set; } = new List<Advert>();

        // Bir kullanıcının birden fazla favorisi olabilir
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}