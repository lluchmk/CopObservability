using Microsoft.EntityFrameworkCore;
using Observability;
using OrdersAPI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

var serviceName = builder.Environment.ApplicationName;
builder.Services.AddObservability(serviceName, builder.Configuration);

builder.Services.AddDbContext<OrdersContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();
app.UseSerilogRequestLogging();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapGet("/orders", async (OrdersContext dbContext, ILogger<Program> logger) =>
{
    logger.AllOrdersRequested();
    return await dbContext.Orders.ToListAsync();
});

app.MapGet("/orders/{id:guid}", async (OrdersContext dbContext, ILogger<Program> logger, Guid id) =>
{
    var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
    if (order is null)
    {
        logger.OrderByIdNotFound(id);
        return Results.NotFound();
    }

    logger.OrderById(order);
    return Results.Ok(order);
});

app.MapGet("/orders/customers/{customerId:guid}", async (OrdersContext dbContext, ILogger<Program> logger, Guid customerId) =>
{
    logger.OrdersByCustomer(customerId);

    var orders = await dbContext.Orders.Where(o => o.CustomerId == customerId).ToListAsync();
    return Results.Ok(orders);
});

app.MapGet("/orders/products/{productId:guid}", async (OrdersContext dbContext, ILogger<Program> logger, Guid productId) =>
{
    logger.OrdersByProduct(productId);

    var orders = await dbContext.Orders.Where(o => o.ProductId == productId).ToListAsync();
    return Results.Ok(orders);
});

app.MapPrometheusScrapingEndpoint();


app.Run();
