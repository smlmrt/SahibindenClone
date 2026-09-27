namespace SahibindenClone.Domain.Entities
{
    public class City : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Bir şehre ait birden fazla ilan olabilir.
        public virtual ICollection<Advert> Adverts { get; set; } = new List<Advert>();
    }
}