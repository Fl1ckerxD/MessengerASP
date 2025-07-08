namespace CorpNetMessenger.Domain.DTOs
{
    public class EmployeeDto
    {
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Patronymic { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = null!;
        public string PostName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; } = string.Empty;
    }
}