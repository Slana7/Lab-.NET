using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Lab4.Validators.Attributes;

public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    public PriceRangeAttribute(double minPrice, double maxPrice)
    {
        // Convert from double to decimal
        _minPrice = (decimal)minPrice;
        _maxPrice = (decimal)maxPrice;

        // Generate error message with currency formatting
        ErrorMessage = $"Price must be between {_minPrice:C} and {_maxPrice:C}";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is decimal price)
        {
            // Implement price range validation
            if (price >= _minPrice && price <= _maxPrice)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }

        // Try to convert to decimal
        if (decimal.TryParse(value.ToString(), NumberStyles.Currency, CultureInfo.CurrentCulture, out var parsedPrice))
        {
            if (parsedPrice >= _minPrice && parsedPrice <= _maxPrice)
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(ErrorMessage);
    }
}


