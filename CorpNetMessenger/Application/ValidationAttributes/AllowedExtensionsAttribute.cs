﻿using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Application.ValidationAttributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions) => _extensions = extensions;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!_extensions.Contains(extension))
                    return new ValidationResult($"Допустимые форматы: {string.Join(", ", _extensions)}");
            }

            return ValidationResult.Success;
        }
    }
}
