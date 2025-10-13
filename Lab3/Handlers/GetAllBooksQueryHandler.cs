using MediatR;
using Microsoft.EntityFrameworkCore;
using Lab3.Queries;
using Lab3.Data;
using Lab3.Models;

namespace Lab3.Handlers;

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
