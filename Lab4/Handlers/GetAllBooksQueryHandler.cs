using MediatR;
using Microsoft.EntityFrameworkCore;
using Lab4.Queries;
using Lab4.Data;
using Lab4.Models;

namespace Lab4.Handlers;

public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, List<Book>>
{
    private readonly BookDbContext _context;

    public GetAllBooksQueryHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        return await _context.Books.ToListAsync(cancellationToken);
    }
}
