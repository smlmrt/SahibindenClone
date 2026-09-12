namespace SahibindenClone.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Üst kategori ID'si (Null ise en üst kategoridir)
        public int? ParentId { get; set; }
        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

        // kategoriye ait ilanlar
        public virtual ICollection<Advert> Adverts { get; set; } = new List<Advert>();
    }
}