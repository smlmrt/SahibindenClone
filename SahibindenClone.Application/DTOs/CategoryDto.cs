namespace SahibindenClone.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new();
    }
}
