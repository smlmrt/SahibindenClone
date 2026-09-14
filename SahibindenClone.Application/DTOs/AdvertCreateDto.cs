namespace SahibindenClone.Application.DTOs
{
    public class AdvertCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
    }
}