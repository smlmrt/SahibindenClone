namespace SahibindenClone.Application.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // kullanıcının sahip olduğu ilanlar
        public List<AdvertListDto> Adverts { get; set; } = new();
    }
}