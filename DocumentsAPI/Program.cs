using CustomersAPI;
using DocumentsAPI;
using Messaging;
using Microsoft.EntityFrameworkCore;
using Observability.Asp;

var builder = WebApplication.CreateBuilder(args);
builder.AddAspObservability(
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
