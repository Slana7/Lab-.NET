namespace Lab4.Common.Logging;

public static class LoggingExtensions
{
    public static void LogBookCreationMetrics(this ILogger logger, BookCreationMetrics metrics)
    {
        var logLevel = metrics.Success ? LogLevel.Information : LogLevel.Error;
        
        logger.Log(
            logLevel,
            LogEvents.BookCreationCompleted,
            "Book creation {Status} - Title: {BookTitle}, ISBN: {Isbn}, Category: {Category}, " +
            "OperationId: {OperationId}, ValidationDuration: {ValidationDurationMs}ms, " +
            "DatabaseSaveDuration: {DatabaseSaveDurationMs}ms, TotalDuration: {TotalDurationMs}ms{ErrorInfo}",
            metrics.Success ? "completed successfully" : "failed",
            metrics.BookTitle,
            metrics.Isbn,
            metrics.Category,
            metrics.OperationId,
            metrics.ValidationDuration.TotalMilliseconds,
            metrics.DatabaseSaveDuration.TotalMilliseconds,
            metrics.TotalDuration.TotalMilliseconds,
            metrics.ErrorReason != null ? $", Error: {metrics.ErrorReason}" : string.Empty);
    }
}


