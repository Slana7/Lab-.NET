using MediatR;
using Lab3.Commands;
using Lab3.Data;
using Lab3.Models;

namespace Lab3.Handlers;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Book>
{
    private readonly BookDbContext _context;

    public CreateBookCommandHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<Book> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Year = request.Year
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        return book;
    }
}
