using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Отчество")]
        public string? Patronymic { get; set; }

        [Required]
        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Пароль должен быть не короче 6 символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердить пароль")]
        public string PasswordConfirm { get; set; }
    }
}
