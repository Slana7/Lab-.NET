using MediatR;
using Lab3.Commands;
using Lab3.Data;

namespace Lab3.Handlers;

public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, bool>
{
    private readonly BookDbContext _context;

    public DeleteBookCommandHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FindAsync(request.Id, cancellationToken);
        
        if (book == null)
            return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
