using CatalogAPI;
using Microsoft.EntityFrameworkCore;
using Observability;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);
builder.AddObservability(configureMetrics: builder =>
{
    builder
        .AddAspNetCoreInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel");
});

builder.Services.AddDbContext<ProductsContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapGet("/products", async (ProductsContext dbContext, ILogger<Program> logger) =>
{
    logger.AllProductsRequested();
    return await dbContext.Products.ToListAsync();
});

app.MapGet("/products/{id:guid}", async (ProductsContext dbContext, ILogger<Program> logger, Guid id) =>
{
    var product = await dbContext.Products.FirstOrDefaultAsync(c => c.Id == id);
    if (product is null)
    {
        logger.ProductByIdNotFound(id);
        return Results.NotFound();
    }

    logger.ProductById(product);
    return Results.Ok(product);
});

app.MapGet("/products/{name}", async (ProductsContext dbContext, ILogger<Program> logger, string name) =>
{
    var product = await dbContext.Products.FirstOrDefaultAsync(c => c.Name.Contains(name));
    if (product is null)
    {
        logger.ProductByNameNotFound(name);
        return Results.NotFound();
    }

    logger.ProductByName(product);
    return Results.Ok(product);
});

app.MapPrometheusScrapingEndpoint();

app.Run();
