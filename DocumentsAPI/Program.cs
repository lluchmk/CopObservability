using CustomersAPI;
using DocumentsAPI;
using Observability;
using RabbitMQ.Client;
using Serilog;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

var serviceName = builder.Environment.ApplicationName;
builder.Services.AddObservability(serviceName, builder.Configuration);

builder.Services.AddScoped(sp =>
{
    var factory = new ConnectionFactory { HostName = builder.Configuration["MessageQueue:HostName"] };
    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();
    channel.QueueDeclare(builder.Configuration["MessageQueue:QueueName"], false, false);
    return channel;
});

var app = builder.Build();
app.UseSerilogRequestLogging();

app.MapPost("/", CreateDocument);

app.MapPrometheusScrapingEndpoint();

app.Run();


Guid CreateDocument(IModel channel, ILogger<Program> logger, CreateDocumentRequest request)
{
    var documentId = Guid.NewGuid();
    logger.RequestedDocument(request, documentId);

    var message = new CreateDocumentMessage(request.Customers, request.Products, request.Errors, documentId);
    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

    channel.BasicPublish(string.Empty, builder.Configuration["MessageQueue:QueueName"], null, body);
    return Guid.NewGuid();
}

