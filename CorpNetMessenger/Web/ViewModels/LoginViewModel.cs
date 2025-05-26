using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
