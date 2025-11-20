using MediatR;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Lab4.Data;
using Lab4.Features.Books.DTOs;
using Lab4.Common.Logging;
using ValidationException = Lab4.Exceptions.ValidationException;
using BookAlreadyExistsException = Lab4.Exceptions.BookAlreadyExistsException;

namespace Lab4.Features.Books;

/// <summary>
/// Command for creating a new book
/// </summary>
public record CreateBookCommand(
    CreateBookProfileRequest Request
) : IRequest<BookProfileDto>;

/// <summary>
/// Handler for creating books with validation, logging, and caching
/// </summary>
public class CreateBookHandler : IRequestHandler<CreateBookCommand, BookProfileDto>
{
    private readonly BookDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CreateBookHandler> _logger;
    private readonly IValidator<CreateBookProfileRequest> _validator;

    public CreateBookHandler(
        BookDbContext context, 
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CreateBookHandler> logger,
        IValidator<CreateBookProfileRequest> validator)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
        _validator = validator;
    }

    public async Task<BookProfileDto> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        // Generate unique 8-character operation ID
        var operationId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var operationStartTime = DateTime.UtcNow;
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Get CreateBookProfileRequest from command
        var createRequest = request.Request;

        // Use logging scope for entire book operation
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["BookTitle"] = createRequest.Title,
            ["Author"] = createRequest.Author,
            ["ISBN"] = createRequest.Isbn,
            ["Category"] = createRequest.Category,
            ["OperationStartTime"] = operationStartTime
        });
        
        // Log operation start with all details
        _logger.LogInformation(LogEvents.BookCreationStarted, 
            "Book creation operation started - Title: {Title}, Author: {Author}, ISBN: {ISBN}, Category: {Category}, OperationId: {OperationId}", 
            createRequest.Title, createRequest.Author, createRequest.Isbn, createRequest.Category, operationId);

        try
        {
            // Start validation phase timing
            var validationStopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Use FluentValidation validator
            var validationResult = await _validator.ValidateAsync(createRequest, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                validationStopwatch.Stop();
                totalStopwatch.Stop();
                
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                
                _logger.LogWarning(LogEvents.BookValidationFailed, 
                    "Book validation failed - Title: {Title}, Author: {Author}, ISBN: {ISBN}, Category: {Category}, Errors: {Errors}",
                    createRequest.Title, createRequest.Author, createRequest.Isbn, createRequest.Category, string.Join("; ", errors));
                
                _logger.LogBookCreationMetrics(new BookCreationMetrics
                {
                    OperationId = operationId,
                    BookTitle = createRequest.Title,
                    Isbn = createRequest.Isbn,
                    Category = createRequest.Category,
                    ValidationDuration = validationStopwatch.Elapsed,
                    DatabaseSaveDuration = TimeSpan.Zero,
                    TotalDuration = totalStopwatch.Elapsed,
                    Success = false,
                    ErrorReason = "Validation failed: " + string.Join(", ", errors)
                });
                
                throw new ValidationException(errors);
            }

            _logger.LogInformation(LogEvents.ISBNValidationPerformed, 
                "All validations passed successfully - ISBN: {ISBN}, Title: {Title}", 
                createRequest.Isbn, createRequest.Title);


            validationStopwatch.Stop();
            
            _logger.LogDebug("Validation phase completed in {ValidationDurationMs}ms", 
                validationStopwatch.Elapsed.TotalMilliseconds);

            // Use advanced mapping for Book creation
            var book = _mapper.Map<Book>(createRequest);

            // Time database operations separately
            _logger.LogInformation(LogEvents.DatabaseOperationStarted, 
                "Starting database save operation - Title: {Title}, ISBN: {ISBN}, Category: {Category}", 
                createRequest.Title, createRequest.Isbn, createRequest.Category);

            var dbStopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            _context.Books.Add(book);
            await _context.SaveChangesAsync(cancellationToken);
            
            dbStopwatch.Stop();

            _logger.LogInformation(LogEvents.DatabaseOperationCompleted, 
                "Database operation completed successfully - BookId: {BookId}, Title: {Title}, ISBN: {ISBN}, Duration: {DatabaseDurationMs}ms", 
                book.Id, book.Title, book.Isbn, dbStopwatch.Elapsed.TotalMilliseconds);

            // Invalidate cache for all books
            _logger.LogInformation(LogEvents.CacheOperationPerformed, 
                "Performing cache invalidation for key: 'all_books'");
            
            _cache.Remove("all_books");
            
            _logger.LogDebug(LogEvents.CacheOperationPerformed, 
                "Cache invalidated successfully for key: 'all_books'");

            // Calculate total operation duration
            totalStopwatch.Stop();

            // Log comprehensive BookCreationMetrics for success case
            _logger.LogBookCreationMetrics(new BookCreationMetrics
            {
                OperationId = operationId,
                BookTitle = book.Title,
                Isbn = book.Isbn,
                Category = book.Category,
                ValidationDuration = validationStopwatch.Elapsed,
                DatabaseSaveDuration = dbStopwatch.Elapsed,
                TotalDuration = totalStopwatch.Elapsed,
                Success = true,
                ErrorReason = null
            });

            // Return BookProfileDto
            return _mapper.Map<BookProfileDto>(book);
        }
        catch (Exception ex) when (ex is not ValidationException && ex is not BookAlreadyExistsException)
        {
            // Log error metrics in catch block with book details
            totalStopwatch.Stop();
            
            _logger.LogError(ex, 
                "Unexpected error during book creation - Title: {Title}, Author: {Author}, ISBN: {ISBN}, Category: {Category}, Error: {ErrorMessage}",
                createRequest.Title, createRequest.Author, createRequest.Isbn, createRequest.Category, ex.Message);
            
            _logger.LogBookCreationMetrics(new BookCreationMetrics
            {
                OperationId = operationId,
                BookTitle = createRequest.Title,
                Isbn = createRequest.Isbn,
                Category = createRequest.Category,
                ValidationDuration = TimeSpan.Zero,
                DatabaseSaveDuration = TimeSpan.Zero,
                TotalDuration = totalStopwatch.Elapsed,
                Success = false,
                ErrorReason = ex.Message
            });
            
            // Re-throw exception for global handler
            throw;
        }
    }
}

// ========== Additional Book Queries and Commands ==========

/// <summary>
/// Query for retrieving a book by ID
/// </summary>
public record GetBookByIdQuery(Guid Id) : IRequest<Book?>;

/// <summary>
/// Handler for retrieving a single book by ID
/// </summary>
public class GetBookByIdHandler : IRequestHandler<GetBookByIdQuery, Book?>
{
    private readonly BookDbContext _context;

    public GetBookByIdHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Books.FindAsync(new object[] { request.Id }, cancellationToken);
    }
}

/// <summary>
/// Query for retrieving all books
/// </summary>
public record GetAllBooksQuery : IRequest<List<Book>>;

/// <summary>
/// Handler for retrieving all books with caching
/// </summary>
public class GetAllBooksHandler : IRequestHandler<GetAllBooksQuery, List<Book>>
{
    private readonly BookDbContext _context;
    private readonly IMemoryCache _cache;

    public GetAllBooksHandler(BookDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Book>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = "all_books";
        
        if (!_cache.TryGetValue(cacheKey, out List<Book>? books))
        {
            books = await Task.Run(() => _context.Books.ToList(), cancellationToken);
            
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            
            _cache.Set(cacheKey, books, cacheOptions);
        }
        
        return books ?? new List<Book>();
    }
}

/// <summary>
/// Command for deleting a book
/// </summary>
public record DeleteBookCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Handler for deleting books
/// </summary>
public class DeleteBookHandler : IRequestHandler<DeleteBookCommand, bool>
{
    private readonly BookDbContext _context;
    private readonly IMemoryCache _cache;

    public DeleteBookHandler(BookDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (book == null)
            return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate cache
        _cache.Remove("all_books");
        
        return true;
    }
}

