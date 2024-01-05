namespace CatalogAPI;

public static partial class Logs
{
    [LoggerMessage(
        EventId = 01,
        Level = LogLevel.Information,
        Message = "Requested all products")]
    public static partial void AllProductsRequested(this ILogger logger);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Information,
        Message = "Requested product by id: {product}")]
    public static partial void ProductById(this ILogger logger, Product product);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Information,
        Message = "Requested product by name: {product}")]
    public static partial void ProductByName(this ILogger logger, Product product);

    [LoggerMessage(
        EventId = 02,
        Level = LogLevel.Warning,
        Message = "Requested unexisting product by id: {id}")]
    public static partial void ProductByIdNotFound(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 03,
        Level = LogLevel.Warning,
        Message = "Requested unexisting product by name: {name}")]
    public static partial void ProductByNameNotFound(this ILogger logger, string name);
}
