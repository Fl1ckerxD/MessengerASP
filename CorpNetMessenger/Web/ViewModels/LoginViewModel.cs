using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите Логин")]
        [Display(Name = "Логин")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Введите Пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
