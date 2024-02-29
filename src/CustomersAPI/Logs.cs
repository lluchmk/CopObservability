namespace CustomersAPI;

public static partial class Logs
{
    [LoggerMessage(
        EventId = 01,
        Level = LogLevel.Information,
        Message = "Requested all customers")]
    public static partial void AllCustomersRequested(this ILogger logger);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Information,
        Message = "Requested customer by id: {customer}")]
    public static partial void CustomerById(this ILogger logger, Customer customer);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Information,
        Message = "Requested customer by name: {customer}")]
    public static partial void CustomerByName(this ILogger logger, Customer customer);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Warning,
        Message = "Requested unexisting customer by id: {id}")]
    public static partial void CustomerByIdNotFound(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Warning,
        Message = "Requested unexisting customer by name: {name}")]
    public static partial void CustomerByNameNotFound(this ILogger logger, string name);
}
