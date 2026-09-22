using System.ComponentModel.DataAnnotations;

namespace SahibindenClone.Application.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        public string Password { get; set; } = string.Empty;
    }
}