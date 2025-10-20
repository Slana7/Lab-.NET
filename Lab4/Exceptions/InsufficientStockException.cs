namespace Lab4.Exceptions;

public class InsufficientStockException : BaseException
{
    public InsufficientStockException(Guid bookId, int available, int requested) 
        : base($"Insufficient stock for book '{bookId}'. Available: {available}, Requested: {requested}", 
               400, "INSUFFICIENT_STOCK")
    {
    }
}

