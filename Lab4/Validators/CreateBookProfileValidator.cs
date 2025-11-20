using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Lab4.Features.Books.DTOs;
using Lab4.Data;
using Lab4.Features.Books;
using System.Text.RegularExpressions;

namespace Lab4.Validators;

public class CreateBookProfileValidator : AbstractValidator<CreateBookProfileRequest>
{
    private readonly BookDbContext _context;
    private readonly ILogger<CreateBookProfileValidator> _logger;
    
    private static readonly string[] InappropriateWords = { "badword1", "badword2", "inappropriate" };
    private static readonly string[] ChildrenRestrictedWords = { "violence", "adult", "explicit", "scary" };
    private static readonly Regex AuthorRegex = new Regex(@"^[a-zA-Z\s\-'.]+$", RegexOptions.Compiled);
    private static readonly Regex IsbnRegex = new Regex(@"^(?:\d{10}|\d{13}|\d{1,5}-\d{1,7}-\d{1,7}-[\dX]|\d{3}-\d{1,5}-\d{1,7}-\d{1,7}-\d)$", RegexOptions.Compiled);
    private static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public CreateBookProfileValidator(BookDbContext context, ILogger<CreateBookProfileValidator> logger)
    {
        _context = context;
        _logger = logger;

        // Title Validation Rules
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required and cannot be empty")
            .Length(1, 200).WithMessage("Title must be between 1 and 200 characters")
            .Must(BeValidTitle)
                .WithMessage("Title contains inappropriate content")
            .MustAsync(async (book, title, cancellation) => await BeUniqueTitle(title, book.Author, cancellation))
                .WithMessage("A book with this title already exists for this author");

        // Author Validation Rules
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required and cannot be empty")
            .Length(2, 100).WithMessage("Author must be between 2 and 100 characters")
            .Must(BeValidAuthorName)
                .WithMessage("Author must contain only letters, spaces, hyphens, apostrophes, and dots");

        // ISBN Validation Rules
        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN is required and cannot be empty")
            .Must(BeValidISBN)
                .WithMessage("ISBN must be a valid format (10 or 13 digits, may contain hyphens)")
            .MustAsync(async (isbn, cancellation) => await BeUniqueISBN(isbn, cancellation))
                .WithMessage("ISBN already exists in the system");

        // Category Validation Rules
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid enum value");

        // Price Validation Rules
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(10000).WithMessage("Price must be less than $10,000");

        // PublishedDate Validation Rules
        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future")
            .Must(date => date.Year >= 1400).WithMessage("Published date cannot be before year 1400");

        // StockQuantity Validation Rules
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000");

        // CoverImageUrl Validation Rules
        RuleFor(x => x.CoverImageUrl)
            .Must(BeValidImageUrl)
                .WithMessage("Cover image URL must be a valid HTTP/HTTPS image URL (.jpg, .jpeg, .png, .gif, .webp)");

        // Business Rules Validation
        RuleFor(x => x)
            .MustAsync(async (book, cancellation) => await PassBusinessRules(book, cancellation))
                .WithMessage("Book does not pass complex business rules validation");
    }

    // Validation Methods
    
    private bool BeValidTitle(string title)
    {
        if (string.IsNullOrEmpty(title)) return true;
        
        var lowerTitle = title.ToLowerInvariant();
        var hasInappropriateContent = InappropriateWords.Any(word => lowerTitle.Contains(word));
        
        if (hasInappropriateContent)
        {
            _logger.LogWarning("Title validation failed: Contains inappropriate content - Title: {Title}", title);
        }
        
        return !hasInappropriateContent;
    }

    private async Task<bool> BeUniqueTitle(string title, string author, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author))
            return true;

        _logger.LogDebug("Checking title uniqueness for author - Title: {Title}, Author: {Author}", title, author);

        var exists = await _context.Books
            .AnyAsync(b => b.Title.ToLower() == title.ToLower() && 
                          b.Author.ToLower() == author.ToLower(), cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Title uniqueness validation failed - Title: {Title}, Author: {Author}", title, author);
        }
        else
        {
            _logger.LogDebug("Title is unique for author - Title: {Title}, Author: {Author}", title, author);
        }

        return !exists;
    }

    private bool BeValidAuthorName(string author)
    {
        if (string.IsNullOrEmpty(author)) return true;
        
        var isValid = AuthorRegex.IsMatch(author);
        
        if (!isValid)
        {
            _logger.LogWarning("Author name validation failed: Invalid characters - Author: {Author}", author);
        }
        
        return isValid;
    }

    private bool BeValidISBN(string isbn)
    {
        if (string.IsNullOrEmpty(isbn)) return true;
        
        var isValid = IsbnRegex.IsMatch(isbn);
        
        if (!isValid)
        {
            _logger.LogWarning("ISBN format validation failed - ISBN: {ISBN}", isbn);
        }
        
        return isValid;
    }

    private async Task<bool> BeUniqueISBN(string isbn, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(isbn))
            return true;

        _logger.LogDebug("Checking ISBN uniqueness - ISBN: {ISBN}", isbn);

        var exists = await _context.Books.AnyAsync(b => b.Isbn == isbn, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("ISBN uniqueness validation failed - ISBN already exists: {ISBN}", isbn);
        }
        else
        {
            _logger.LogDebug("ISBN is unique - ISBN: {ISBN}", isbn);
        }

        return !exists;
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Image URL validation failed: Invalid URI format - URL: {URL}", url);
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            _logger.LogWarning("Image URL validation failed: Invalid protocol (must be HTTP/HTTPS) - URL: {URL}", url);
            return false;
        }

        var path = uri.AbsolutePath.ToLowerInvariant();
        var hasValidExtension = ValidImageExtensions.Any(ext => path.EndsWith(ext));
        
        if (!hasValidExtension)
        {
            _logger.LogWarning("Image URL validation failed: Invalid image extension - URL: {URL}", url);
        }

        return hasValidExtension;
    }

    private async Task<bool> PassBusinessRules(CreateBookProfileRequest book, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting business rules validation - Title: {Title}, ISBN: {ISBN}, Category: {Category}", 
            book.Title, book.Isbn, book.Category);

        // Rule 1: Daily book addition limit check (max 500 per day)
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);
        
        var todayBookCount = await _context.Books
            .CountAsync(b => b.CreatedAt >= todayStart && b.CreatedAt < todayEnd, cancellationToken);

        if (todayBookCount >= 500)
        {
            _logger.LogWarning("Business Rule 1 failed: Daily book addition limit exceeded - Current count: {Count}, Max: 500", todayBookCount);
            return false;
        }
        
        _logger.LogDebug("Business Rule 1 passed: Daily book count check - Current: {Count}/500", todayBookCount);

        // Rule 2: Technical books minimum price check ($20.00)
        if (book.Category == BookCategory.Technical && book.Price < 20.00m)
        {
            _logger.LogWarning("Business Rule 2 failed: Technical book price too low - Price: {Price}, Minimum: $20.00", book.Price);
            return false;
        }
        
        if (book.Category == BookCategory.Technical)
        {
            _logger.LogDebug("Business Rule 2 passed: Technical book price check - Price: {Price}", book.Price);
        }

        // Rule 3: Children's book content restrictions (check title against restricted words)
        if (book.Category == BookCategory.Children)
        {
            var lowerTitle = book.Title.ToLowerInvariant();
            var hasRestrictedContent = ChildrenRestrictedWords.Any(word => lowerTitle.Contains(word));
            
            if (hasRestrictedContent)
            {
                _logger.LogWarning("Business Rule 3 failed: Children's book contains restricted content - Title: {Title}", book.Title);
                return false;
            }
            
            _logger.LogDebug("Business Rule 3 passed: Children's book content check - Title: {Title}", book.Title);
        }

        // Rule 4: High-value book stock limit (>$500 = max 10 stock)
        if (book.Price > 500 && book.StockQuantity > 10)
        {
            _logger.LogWarning("Business Rule 4 failed: High-value book has excessive stock - Price: {Price}, Stock: {Stock}, Max: 10", 
                book.Price, book.StockQuantity);
            return false;
        }
        
        if (book.Price > 500)
        {
            _logger.LogDebug("Business Rule 4 passed: High-value book stock check - Price: {Price}, Stock: {Stock}", 
                book.Price, book.StockQuantity);
        }

        _logger.LogInformation("All business rules passed - Title: {Title}, ISBN: {ISBN}", book.Title, book.Isbn);
        
        return true;
    }
}

