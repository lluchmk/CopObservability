using Azure.Storage.Blobs;
using DocumentsGenerator;
using Messaging;
using Observability;
using OpenTelemetry.Metrics;

var builder = Host.CreateApplicationBuilder(args);

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
builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<ResumeGenerator>();
builder.Services.AddScoped<DocumentsRepository>();

builder.Services.AddScoped(sp =>
{
    return new BlobServiceClient(builder.Configuration["BlobStorageConnectionString"]);
});

var host = builder.Build();
host.Run();
