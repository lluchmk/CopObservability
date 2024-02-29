using DocumentsAPI;

namespace CustomersAPI;

public static partial class Logs
{
    [LoggerMessage(
        EventId = 01,
        Level = LogLevel.Information,
        Message = "Requested document creation: {request}. Assigned id :{id}")]
    public static partial void RequestedDocumentCreation(this ILogger logger, CreateDocumentRequest request, Guid id);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Information,
        Message = "Requested document with id :{id}")]
    public static partial void RequestedDocument(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Information,
        Message = "Requested unexisting document with id :{id}")]
    public static partial void RequestedUnexistingDocument(this ILogger logger, Guid id);
}
