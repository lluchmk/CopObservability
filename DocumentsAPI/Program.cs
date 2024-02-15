using CustomersAPI;
using DocumentsAPI;
using Messaging;
using Microsoft.EntityFrameworkCore;
using Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.AddObservability(configureMetrics: builder =>
{
    builder
        .AddAspNetCoreInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel");
},
configureTracing: builder =>
{
    builder.AddSource(nameof(MessageSender));
});

builder.Services.AddOptions<RabbitMqSettings>()
    .BindConfiguration(nameof(RabbitMqSettings));

builder.Services.AddSingleton<MessageSender>();

builder.Services.AddDbContext<DocumentContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();

app.UseTracingExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DocumentContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapPost("/", CreateDocument);

app.MapGet("/{documentId:Guid}", async (DocumentContext context, ILogger<Program> logger, Guid documentId) =>
{
    var document = await context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
    if (document is null)
    {
        logger.RequestedUnexistingDocument(documentId);
        return Results.NotFound();
    }

    logger.RequestedDocument(documentId);
    return Results.Ok(document);
});

app.MapPrometheusScrapingEndpoint();

app.Run();

async Task<IResult> CreateDocument(DocumentContext context, MessageSender messageSender, ILogger<Program> logger, CreateDocumentRequest request)
{
    var document = new Document
    {
        Status = DocumentStatus.Requested
    };
    await context.Documents.AddAsync(document);
    await context.SaveChangesAsync();

    logger.RequestedDocumentCreation(request, document.Id);

    var message = new CreateDocumentMessage(request.Customers, request.Products, request.Errors, document.Id);
    messageSender.SendMessage(message);

    return Results.Ok(document);
}
