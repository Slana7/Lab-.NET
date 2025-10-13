using MediatR;
using Lab3.Queries;
using Lab3.Data;
using Lab3.Models;

namespace Lab3.Handlers;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, Book?>
{
    private readonly BookDbContext _context;

    public GetBookByIdQueryHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Books.FindAsync(request.Id, cancellationToken);
    }
}
