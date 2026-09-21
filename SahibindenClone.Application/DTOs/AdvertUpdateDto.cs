using System.ComponentModel.DataAnnotations;

namespace SahibindenClone.Application.DTOs
{
    public class AdvertUpdateDto
    {
        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        [MaxLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
        [MinLength(3, ErrorMessage = "Başlık en az 3 karakter olmalıdır.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [MaxLength(5000, ErrorMessage = "Açıklama en fazla 5000 karakter olabilir.")]
        [MinLength(10, ErrorMessage = "Açıklama en az 10 karakter olmalıdır.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0, 999_999_999, ErrorMessage = "Fiyat 0 ile 999.999.999 TL arasında olmalıdır.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçiniz.")]
        public int CategoryId { get; set; }
    }
}
