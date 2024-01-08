using Microsoft.Extensions.Logging;

namespace Messaging;

public static partial class Logs
{
    [LoggerMessage(
        EventId = 01,
        Level = LogLevel.Information,
        Message = "Message sent: {message}")]
    public static partial void MessageSent(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Error,
        Message = "Failure to send message.")]
    public static partial void MessageException(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Error,
        Message = "Failed to inject trace context.")]
    public static partial void ContextInjectException(this ILogger logger, Exception ex);

    [LoggerMessage(
       EventId = 04,
       Level = LogLevel.Error,
       Message = "Invalid message received: {message}")]
    public static partial void InvalidMessageReceived(this ILogger logger, string message);

    [LoggerMessage(
       EventId = 05,
       Level = LogLevel.Information,
       Message = "Received message: {message}")]
    public static partial void MessageReceived(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 06,
        Level = LogLevel.Error,
        Message = "Failed to retrieve trace context.")]
    public static partial void ContextRetrieveException(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 07,
        Level = LogLevel.Error,
        Message = "Failed to process message.")]
    public static partial void ProcessMessageException(this ILogger logger, Exception ex);
}
