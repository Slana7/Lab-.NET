using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;
using FluentValidation;
using Lab4.Features.Books;
using Lab4.Data;
using Lab4.Features.Books.DTOs;
using Lab4.Common.Mapping;
using Lab4.Validators;
using Lab4.Common.Logging;

namespace Lab4.Tests;

public class CreateBookHandlerIntegrationTests : IDisposable
{
    private readonly BookDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateBookHandler>> _loggerMock;
    private readonly IValidator<CreateBookProfileRequest> _validator;
    private readonly CreateBookHandler _handler;

    public CreateBookHandlerIntegrationTests()
    {
        // Set up in-memory database with unique name
        var options = new DbContextOptionsBuilder<BookDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new BookDbContext(options);

        // Configure AutoMapper with both book profiles
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdvancedBookMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Set up memory cache
        _cache = new MemoryCache(new MemoryCacheOptions());

        // Mock ILogger<CreateBookHandler>
        _loggerMock = new Mock<ILogger<CreateBookHandler>>();

        // Set up validator
        var validatorLoggerMock = new Mock<ILogger<CreateBookProfileValidator>>();
        _validator = new CreateBookProfileValidator(_context, validatorLoggerMock.Object);

        // Create handler instance with all dependencies
        _handler = new CreateBookHandler(
            _context,
            _mapper,
            _cache,
            _loggerMock.Object,
            _validator
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _cache.Dispose();
    }

    [Fact]
    public async Task Handle_ValidTechnicalBookRequest_CreatesBookWithCorrectMappings()
    {
        // Arrange
        var request = new CreateBookProfileRequest
        {
            Title = "Advanced Software Engineering",
            Author = "John Smith",
            Isbn = "9780123456789",
            Category = BookCategory.Technical,
            Price = 49.99m,
            PublishedDate = DateTime.UtcNow.AddYears(-2),
            CoverImageUrl = "https://example.com/cover.jpg",
            StockQuantity = 15
        };
        var command = new CreateBookCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Technical & Professional", result.CategoryDisplayName);
        Assert.Equal("JS", result.AuthorInitials);
        Assert.Contains("years", result.PublishedAge);
        Assert.StartsWith("$", result.FormattedPrice);
        Assert.Equal("In Stock", result.AvailabilityStatus);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                LogEvents.BookCreationStarted,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
    {
        // Arrange: Create existing book in database
        var existingBook = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Existing Book",
            Author = "Jane Doe",
            Isbn = "9780987654321",
            Category = BookCategory.Fiction,
            Price = 29.99m,
            Year = 2020,
            PublishedDate = DateTime.UtcNow.AddYears(-3),
            CreatedAt = DateTime.UtcNow,
            IsAvailable = true,
            StockQuantity = 10
        };
        _context.Books.Add(existingBook);
        await _context.SaveChangesAsync();

        // Arrange: Create request with same ISBN
        var request = new CreateBookProfileRequest
        {
            Title = "Duplicate Book",
            Author = "John Smith",
            Isbn = "9780987654321",
            Category = BookCategory.Fiction,
            Price = 39.99m,
            PublishedDate = DateTime.UtcNow.AddYears(-1),
            StockQuantity = 5
        };
        var command = new CreateBookCommand(request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Lab4.Exceptions.ValidationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.Contains("already exists", StringComparison.OrdinalIgnoreCase));
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                LogEvents.BookValidationFailed,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ChildrensBookRequest_AppliesDiscountAndConditionalMapping()
    {
        // Arrange
        var request = new CreateBookProfileRequest
        {
            Title = "The Happy Adventure",
            Author = "Mary Johnson",
            Isbn = "9781111111111",
            Category = BookCategory.Children,
            Price = 20.00m,
            PublishedDate = DateTime.UtcNow.AddYears(-1),
            CoverImageUrl = "https://example.com/kids-cover.jpg",
            StockQuantity = 50
        };
        var command = new CreateBookCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Children's Books", result.CategoryDisplayName);
        Assert.Equal(18.00m, result.Price); // 10% discount: 20 * 0.9 = 18
        Assert.Null(result.CoverImageUrl); // Content filtering for children
    }
}

