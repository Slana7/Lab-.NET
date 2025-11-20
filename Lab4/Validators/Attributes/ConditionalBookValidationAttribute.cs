using System.ComponentModel.DataAnnotations;
using Lab4.Features.Books;

namespace Lab4.Validators.Attributes;

public class ConditionalBookValidationAttribute : ValidationAttribute
{
    private static readonly string[] TechnicalKeywords = 
    { 
        "programming", "software", "code", "algorithm", "data", "computer", 
        "technology", "engineering", "development", "system", "design", "architecture"
    };
    
    private static readonly string[] InappropriateWordsForChildren = 
    { 
        "violence", "death", "kill", "murder", "blood", "adult", "explicit", 
        "horror", "scary", "terror", "war", "weapon"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var request = value as dynamic;
        if (request == null)
        {
            return ValidationResult.Success;
        }

        var errors = new List<string>();

        // Get properties using reflection
        var categoryProp = value.GetType().GetProperty("Category");
        var priceProp = value.GetType().GetProperty("Price");
        var titleProp = value.GetType().GetProperty("Title");
        var authorProp = value.GetType().GetProperty("Author");
        var publishedDateProp = value.GetType().GetProperty("PublishedDate");
        var stockQuantityProp = value.GetType().GetProperty("StockQuantity");

        if (categoryProp == null || priceProp == null || titleProp == null)
        {
            return ValidationResult.Success;
        }

        var category = (BookCategory)categoryProp.GetValue(value)!;
        var price = (decimal)priceProp.GetValue(value)!;
        var title = (string?)titleProp.GetValue(value) ?? string.Empty;
        var author = (string?)authorProp?.GetValue(value) ?? string.Empty;
        var publishedDate = publishedDateProp != null ? (DateTime)publishedDateProp.GetValue(value)! : DateTime.MinValue;
        var stockQuantity = stockQuantityProp != null ? (int)stockQuantityProp.GetValue(value)! : 0;

        // Technical Book Conditions
        if (category == BookCategory.Technical)
        {
            // Price minimum $20.00
            if (price < 20.00m)
            {
                errors.Add("Technical books must have a minimum price of $20.00");
            }

            // Must contain technical keywords in title
            if (!ContainsTechnicalKeywords(title))
            {
                errors.Add("Technical books must contain technical keywords in the title");
            }

            // Must be published within last 5 years
            if (publishedDate != DateTime.MinValue && (DateTime.UtcNow - publishedDate).TotalDays > 365 * 5)
            {
                errors.Add("Technical books must be published within the last 5 years");
            }
        }

        // Children's Book Conditions
        if (category == BookCategory.Children)
        {
            // Price maximum $50.00
            if (price > 50.00m)
            {
                errors.Add("Children's books must have a maximum price of $50.00");
            }

            // Title must be appropriate for children
            if (!IsAppropriateForChildren(title))
            {
                errors.Add("Children's book title contains inappropriate content");
            }
        }

        // Fiction Book Conditions
        if (category == BookCategory.Fiction)
        {
            // Author name minimum 5 characters (full name requirement)
            if (author.Length < 5)
            {
                errors.Add("Fiction books require author full name (minimum 5 characters)");
            }
        }

        // Cross-Field Validation: Expensive books must have limited stock
        if (price > 100 && stockQuantity > 20)
        {
            errors.Add("Expensive books (>$100) must have limited stock (≤20 units)");
        }

        if (errors.Any())
        {
            return new ValidationResult(string.Join("; ", errors));
        }

        return ValidationResult.Success;
    }

    // Helper Methods

    private bool ContainsTechnicalKeywords(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        var lowerTitle = title.ToLowerInvariant();
        return TechnicalKeywords.Any(keyword => lowerTitle.Contains(keyword));
    }

    private bool IsAppropriateForChildren(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return true;
        }

        var lowerTitle = title.ToLowerInvariant();
        return !InappropriateWordsForChildren.Any(word => lowerTitle.Contains(word));
    }
}


