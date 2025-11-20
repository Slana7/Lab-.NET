using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Lab4.Validators.Attributes;

public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
{
    private static readonly Regex IsbnRegex = new Regex(@"^\d{10}$|^\d{13}$", RegexOptions.Compiled);

    public ValidISBNAttribute()
    {
        ErrorMessage = "ISBN must be 10 or 13 digits";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var isbn = value.ToString()!;
        
        // Remove hyphens and spaces before validation
        isbn = isbn.Replace("-", "").Replace(" ", "").Trim();

        if (IsbnRegex.IsMatch(isbn))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Add data attributes for client-side validation
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-isbn"] = ErrorMessage ?? "Invalid ISBN format";
        context.Attributes["data-val-isbn-pattern"] = @"^\d{10}$|^\d{13}$";
    }
}


