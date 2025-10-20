namespace Lab4.Exceptions;

public class DatabaseException : BaseException
{
    public DatabaseException(string message, Exception innerException) 
        : base($"Database operation failed: {message}", innerException, 500, "DATABASE_ERROR")
    {
    }
}

