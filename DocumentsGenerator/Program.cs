using Azure.Storage.Blobs;
using DocumentsGenerator;
using Observability;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.AddSerilog();

builder.Services.AddHostedService<Worker>();

var serviceName = builder.Environment.ApplicationName;
builder.Services.AddObservability(serviceName, builder.Configuration);

builder.Services.AddHttpClient<OrdersClient>(opt => opt.BaseAddress = new Uri(builder.Configuration["OrdersApiUrl"]!));
builder.Services.AddHttpClient<CatalogClient>(opt => opt.BaseAddress = new Uri(builder.Configuration["CatalogApiUrl"]!));
builder.Services.AddHttpClient<CustomersClient>(opt => opt.BaseAddress = new Uri(builder.Configuration["CustomersApiUrl"]!));
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped(sp =>
{
    return new BlobServiceClient(builder.Configuration["BlobStorageConnectionString"]);
});

var host = builder.Build();
host.Run();
