using CustomersAPI;
using DocumentsAPI;
using Messaging;
using Observability;
using OpenTelemetry.Metrics;

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

var app = builder.Build();

app.MapPost("/", CreateDocument);

app.MapPrometheusScrapingEndpoint();

app.Run();

Guid CreateDocument(MessageSender messageSender, ILogger<Program> logger, CreateDocumentRequest request)
{
    var documentId = Guid.NewGuid();
    logger.RequestedDocument(request, documentId);

    var message = new CreateDocumentMessage(request.Customers, request.Products, request.Errors, documentId);
    messageSender.SendMessage(message);

    return documentId;
}

