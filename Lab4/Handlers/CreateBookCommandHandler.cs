using MediatR;
using Lab4.Commands;
using Lab4.Data;
using Lab4.Models;
using Lab4.Exceptions;

namespace Lab4.Handlers;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Book>
{
    private readonly BookDbContext _context;

    public CreateBookCommandHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<Book> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Title))
            errors.Add("Title is required");
        
        if (string.IsNullOrWhiteSpace(request.Author))
            errors.Add("Author is required");
        
        if (request.Year < 1000 || request.Year > DateTime.UtcNow.Year + 1)
            errors.Add($"Year must be between 1000 and {DateTime.UtcNow.Year + 1}");
        
        if (errors.Any())
            throw new ValidationException(errors);

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Author = request.Author,
            Year = request.Year,
            Isbn = $"ISBN-{Guid.NewGuid().ToString()[..13].ToUpper()}",
            Category = BookCategory.Fiction,
            Price = 0m,
            PublishedDate = new DateTime(request.Year, 1, 1),
            IsAvailable = true,
            StockQuantity = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        return book;
    }
}
