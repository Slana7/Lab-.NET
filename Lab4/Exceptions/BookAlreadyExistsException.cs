namespace Lab4.Exceptions;

public class BookAlreadyExistsException : BaseException
{
    public BookAlreadyExistsException(string isbn) 
        : base($"Book with ISBN '{isbn}' already exists", 409, "BOOK_ALREADY_EXISTS")
    {
    }
}

