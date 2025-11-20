using System.ComponentModel.DataAnnotations;
using Lab4.Features.Books;

namespace Lab4.Validators.Attributes;

public class BookCategoryAttribute : ValidationAttribute
{
    private readonly BookCategory[] _allowedCategories;

    public BookCategoryAttribute(params BookCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
        
        // Generate error message with allowed categories list
        var categoryNames = string.Join(", ", _allowedCategories.Select(c => c.ToString()));
        ErrorMessage = $"Category must be one of: {categoryNames}";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is BookCategory category)
        {
            // Check category against allowed list
            if (_allowedCategories.Contains(category))
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(ErrorMessage);
    }
}


