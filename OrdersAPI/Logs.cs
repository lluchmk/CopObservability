namespace OrdersAPI;

public static partial class Logs
{
    [LoggerMessage(
        EventId = 01,
        Level = LogLevel.Information,
        Message = "Requested all orders")]
    public static partial void AllOrdersRequested(this ILogger logger);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Information,
        Message = "Requested order by id: {order}")]
    public static partial void OrderById(this ILogger logger, Order order);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Information,
        Message = "Requested orders for customer: {customerId}")]
    public static partial void OrdersByCustomer(this ILogger logger, Guid customerId);

    [LoggerMessage(
        EventId = 04,
        Level = LogLevel.Information,
        Message = "Requested orders for product: {productId}")]
    public static partial void OrdersByProduct(this ILogger logger, Guid productId);

    [LoggerMessage(
        EventId = 05,
        Level = LogLevel.Warning,
        Message = "Requested unexisting order by id: {id}")]
    public static partial void OrderByIdNotFound(this ILogger logger, Guid id);
}
