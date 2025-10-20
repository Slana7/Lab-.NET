using MediatR;
using Lab4.Queries;
using Lab4.Data;
using Lab4.Models;

namespace Lab4.Handlers;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, Book?>
{
    private readonly BookDbContext _context;

    public GetBookByIdQueryHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Books.FindAsync(new object[] { request.Id }, cancellationToken);
    }
}
