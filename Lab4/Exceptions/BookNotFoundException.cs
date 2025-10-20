namespace Lab4.Exceptions;

public class BookNotFoundException : BaseException
{
    public BookNotFoundException(Guid bookId) 
        : base($"Book with ID '{bookId}' was not found", 404, "BOOK_NOT_FOUND")
    {
    }
}

