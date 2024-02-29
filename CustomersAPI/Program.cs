using CustomersAPI;
using Microsoft.EntityFrameworkCore;
using Observability.Asp;

var builder = WebApplication.CreateBuilder(args);
builder.AddAspObservability();

builder.Services.AddDbContext<CustomersContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();

app.UseTracingExceptionHandler();

app.MapGet("/customers", async (CustomersContext dbContext, ILogger<Program> logger) =>
{
    logger.AllCustomersRequested();
    return await dbContext.Customers.ToListAsync();
});

app.MapGet("/customers/{id:guid}", async (CustomersContext dbContext, ILogger<Program> logger, Guid id) =>
{
    var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
    if (customer is null)
    {
        logger.CustomerByIdNotFound(id);
        return Results.NotFound();
    }

    logger.CustomerById(customer);
    return Results.Ok(customer);
});

app.MapGet("/customers/{name}", async (CustomersContext dbContext, ILogger<Program> logger, string name) =>
{
    if ("error".Equals(name, StringComparison.OrdinalIgnoreCase))
    {
        throw new Exception("An error has occurred requesting the customer. How unexpected.");
    }

    var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Name.Contains(name));
    if (customer is null)
    {
        logger.CustomerByNameNotFound(name);
        return Results.NotFound();
    }

    logger.CustomerByName(customer);
    return Results.Ok(customer);
});

app.MapPrometheusScrapingEndpoint();

app.Run();
