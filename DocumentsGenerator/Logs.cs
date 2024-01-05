namespace DocumentsGenerator;

public static partial class Logs
{
    [LoggerMessage(
        EventId = 01,
        Level = LogLevel.Information,
        Message = "Received message: {message}")]
    public static partial void MessageReceived(this ILogger logger, CreateDocumentMessage message);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Error,
        Message = "Invalid message received: {message}")]
    public static partial void InvalidMessageReceived(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Warning,
        Message = "Empty request received")]
    public static partial void EmptyRequestReceived(this ILogger logger);

    [LoggerMessage(
        EventId = 04,
        Level = LogLevel.Information,
        Message = "Creating file: {fileName}")]
    public static partial void CreatingFile(this ILogger logger, string fileName);

    [LoggerMessage(
        EventId = 05,
        Level = LogLevel.Information,
        Message = "Uploading file to blob storage")]
    public static partial void UploadingToBlob(this ILogger logger);

    [LoggerMessage(
        EventId = 06,
        Level = LogLevel.Error,
        Message = "Error creating file: {message}")]
    public static partial void ErrorCreatingFile(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 06,
        Level = LogLevel.Error,
        Message = "Error uploading file: {message}")]
    public static partial void ErrorUploadingFile(this ILogger logger, string message);
}
