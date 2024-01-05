using DocumentsAPI;

namespace CustomersAPI;

public static partial class Logs
{
    [LoggerMessage(
        EventId = 01,
        Level = LogLevel.Information,
        Message = "Requested document creation: {request}. Assigned id :{id}")]
    public static partial void RequestedDocument(this ILogger logger, CreateDocumentRequest request, Guid id);
}
