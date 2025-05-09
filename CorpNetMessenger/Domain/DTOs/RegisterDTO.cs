namespace CorpNetMessenger.Domain.DTOs
{
    public class RegisterDTO
    {
        public string LastName { get; set; }
        public string Name { get; set; }
        public string? Patronymic { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}
