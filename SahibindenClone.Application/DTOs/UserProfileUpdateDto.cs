using System.ComponentModel.DataAnnotations;

namespace SahibindenClone.Application.DTOs
{
    public class UserProfileUpdateDto
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [MinLength(2, ErrorMessage = "Ad en az 2 karakter olmalıdır.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [MinLength(2, ErrorMessage = "Soyad en az 2 karakter olmalıdır.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        // Şifre güncellemeleri için
        public string? CurrentPassword { get; set; }
        
        [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
        public string? NewPassword { get; set; }
    }
}