using Lab4.Features.Books;
using Lab4.Validators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Lab4.Features.Books.DTOs;

/// <summary>
/// Response DTO for book profiles with advanced mapping
/// </summary>
public class BookProfileDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string CategoryDisplayName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string FormattedPrice { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public string PublishedAge { get; set; } = string.Empty;
    public string AuthorInitials { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for creating book profiles with validation
/// </summary>
[ConditionalBookValidation]
public class CreateBookProfileRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Author is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Author must be between 2 and 100 characters")]
    public string Author { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ISBN is required")]
    [ValidISBN]
    public string Isbn { get; set; } = string.Empty;
    
    [BookCategory(BookCategory.Fiction, BookCategory.NonFiction, BookCategory.Technical, BookCategory.Children)]
    public BookCategory Category { get; set; }
    
    [PriceRange(0.01, 10000)]
    public decimal Price { get; set; }
    
    public DateTime PublishedDate { get; set; }
    public string? CoverImageUrl { get; set; }
    
    [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100,000")]
    public int StockQuantity { get; set; } = 1;
}

