using MigrationsRunner;
using Observability;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var openTelemetrySettings = builder.Configuration
           .GetSection(nameof(OpenTelemetrySettings))
           .Get<OpenTelemetrySettings>();

builder.AddObservability();

var host = builder.Build();

host.Run();
