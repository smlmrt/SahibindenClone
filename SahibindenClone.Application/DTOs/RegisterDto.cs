using System.ComponentModel.DataAnnotations;

namespace SahibindenClone.Application.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [MinLength(2, ErrorMessage = "Ad en az 2 karakter olmalıdır.")]
        [MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [MinLength(2, ErrorMessage = "Soyad en az 2 karakter olmalıdır.")]
        [MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", 
            ErrorMessage = "Şifreniz en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
        [Compare("Password", ErrorMessage = "Girdiğiniz şifreler birbiriyle eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}