using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Application.ValidationAttributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;

        public MaxFileSizeAttribute(int maxSize) => _maxSize = maxSize;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > _maxSize)
                return new ValidationResult($"Максимальный размер файла: {_maxSize / 1024 / 1024}MB");

            return ValidationResult.Success;
        }
    }
}
