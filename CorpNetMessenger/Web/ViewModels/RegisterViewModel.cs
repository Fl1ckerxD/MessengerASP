using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Web.ViewModels
{
    public class RegisterViewModel
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

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина логина 3-20 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Только буквы, цифры и подчеркивания")]
        [Remote("IsUserNameUnique", "Validation", ErrorMessage = "Логин занят")]
        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        [Remote("IsEmailUnique", "Validation", ErrorMessage = "Email уже занят")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Телефон")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [StringLength(100, ErrorMessage = "Пароль должен быть не короче 6 символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердить пароль")]
        public string PasswordConfirm { get; set; }

        [Required(ErrorMessage = "Выберите отдел")]
        [Display(Name = "Отдел")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Выберите должность")]
        [Display(Name = "Должность")]
        public int PostId { get; set; }
    }
}
