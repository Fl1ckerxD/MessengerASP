using CorpNetMessenger.Application.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Web.ViewModels
{
    public class AccountViewModel
    {
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Длина фамилии 2-30 символов")]
        [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\-]+$", ErrorMessage = "Только буквы и дефисы")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Длина имени 2-20 символов")]
        [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z]+$", ErrorMessage = "Только буквы")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [MaxLength(30, ErrorMessage = "Длина отчества до 30 символов")]
        [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z]+$", ErrorMessage = "Только буквы")]
        [Display(Name = "Отчество")]
        public string? Patronymic { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        [Remote("IsEmailUnique", "Validation", AdditionalFields = "Id", ErrorMessage = "Email уже занят")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Телефон")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Аватар")]
        [DataType(DataType.Upload)]
        [MaxFileSize(5 * 1024 * 1024)] // 5MB
        [AllowedExtensions([".jpg", ".jpeg", ".png"])]
        public IFormFile? AvatarFile { get; set; }

        [StringLength(100, ErrorMessage = "Пароль должен быть не короче 6 символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string? Password { get; set; }

        [StringLength(100, ErrorMessage = "Пароль должен быть не короче 6 символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердить новый пароль")]
        public string? PasswordConfirm { get; set; }
    }
}
