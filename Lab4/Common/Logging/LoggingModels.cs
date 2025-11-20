using Lab4.Features.Books;

namespace Lab4.Common.Logging;

/// <summary>
/// Metrics for book creation operations
/// </summary>
public record BookCreationMetrics
{
    public string OperationId { get; init; } = string.Empty;
    public string BookTitle { get; init; } = string.Empty;
    public string Isbn { get; init; } = string.Empty;
    public BookCategory Category { get; init; }
    public TimeSpan ValidationDuration { get; init; }
    public TimeSpan DatabaseSaveDuration { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public bool Success { get; init; }
    public string? ErrorReason { get; init; }
}

/// <summary>
/// Event IDs for structured logging
/// </summary>
public static class LogEvents
{
    public const int BookCreated = 1000;
    public const int BookValidationFailed = 1001;
    public const int BookDatabaseError = 1002;
    public const int BookNotFound = 1003;
    public const int BookDuplicate = 1004;
    public const int BookRetrieved = 1005;
    public const int BookDeleted = 1006;
    public const int BookListRetrieved = 1007;
    
    // Additional operation events
    public const int BookCreationStarted = 1010;
    public const int ISBNValidationPerformed = 1011;
    public const int DatabaseOperationStarted = 1012;
    public const int DatabaseOperationCompleted = 1013;
    public const int CacheOperationPerformed = 1014;
    public const int BookCreationCompleted = 1015;
}

