using Azure.Storage.Blobs;
using DocumentsGenerator;
using Messaging;
using Microsoft.AspNetCore.Builder;
using Observability;
using OpenTelemetry.Metrics;

var builder = Host.CreateApplicationBuilder(args);

//var documentsGeneratorMeter = new Meter("DocumentsGenerator");
//var customersCounter = documentsGeneratorMeter.CreateCounter<int>("documents.customers", description: "Counts the amount of times reports are generated for a customer");
//var productsCounter = documentsGeneratorMeter.CreateCounter<int>("documents.products", description: "Counts the amount of times reports are generated for a product");

var openTelemetrySettings = builder.Configuration
           .GetSection(nameof(OpenTelemetrySettings))
           .Get<OpenTelemetrySettings>();

builder.AddObservability(configureMetrics: builder =>
{
    builder.AddMeter("DocumentsGenerator");
    builder.AddOtlpExporter(otlpExporterOPtions =>
    {
        otlpExporterOPtions.Endpoint = openTelemetrySettings!.Endpoint;
    });
},
configureTracing: builder =>
{
    builder.AddSource(nameof(MessageReceiver));
});

builder.Services.AddHostedService<Worker>();

builder.Services.AddOptions<RabbitMqSettings>()
    .BindConfiguration(nameof(RabbitMqSettings));

builder.Services.AddSingleton<MessageReceiver>();

builder.Services.AddHttpClient<OrdersClient>(opt => opt.BaseAddress = new Uri(builder.Configuration["OrdersApiUrl"]!));
builder.Services.AddHttpClient<CatalogClient>(opt => opt.BaseAddress = new Uri(builder.Configuration["CatalogApiUrl"]!));
builder.Services.AddHttpClient<CustomersClient>(opt => opt.BaseAddress = new Uri(builder.Configuration["CustomersApiUrl"]!));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<RequestsMetrics>();

builder.Services.AddScoped(sp =>
{
    return new BlobServiceClient(builder.Configuration["BlobStorageConnectionString"]);
});

var host = builder.Build();
host.Run();
