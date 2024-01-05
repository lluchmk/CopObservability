using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

// Custom metrics for the application
var greeterMeter = new Meter("CoP.NetObservability", "1.0.0");
var countGreetings = greeterMeter.CreateCounter<int>("greetings.count", description: "counts the number of greetings");

// Custom ActivitySource for the application
var greeterActivitySource = new ActivitySource("CoP.NetObservability");

var builder = WebApplication.CreateBuilder(args);

var tracingOtlpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];
var otel = builder.Services.AddOpenTelemetry();

// Configure OpenTelemetry resources with the application name
otel.ConfigureResource(res => res.AddService(serviceName: builder.Environment.ApplicationName));

// Add metrics for ASP.NET Core and out custom metrics and export to Prometheus
otel.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddMeter(greeterMeter.Name)
    // Metrics provided by ASP.NET Core in .NET 8
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter()
);

// Add tracing for ASP.NET Core and our custom ActivitySoure and export to Jaeger
otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(greeterActivitySource.Name);

    if (!string.IsNullOrWhiteSpace(tracingOtlpEndpoint))
    {
        tracing.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
        });
    }
    else
    {
        tracing.AddConsoleExporter();
    }
});

var app = builder.Build();

app.MapGet("/", SendGreeting);

// Configure the Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint();

app.Run();

string SendGreeting(ILogger<Program> logger)
{
    // Create a new Activity scoped to the method
    using var activity = greeterActivitySource.StartActivity("GreeterActivity");

    // Log a message
    logger.LogInformation("Sending greeting");

    // Increment the custom counter
    countGreetings.Add(1);

    // Add a tag to the Activity
    activity?.SetTag("greeting", "Hello world!");

    return "Hello world!";
}