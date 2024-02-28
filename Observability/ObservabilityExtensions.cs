using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IHostApplicationBuilder builder
        , Action<MeterProviderBuilder>? configureMetrics = null
        , Action<TracerProviderBuilder>? configureTracing = null)
    {
        var serviceName = builder.Environment.ApplicationName;
        var openTelemetrySettings = builder.Configuration
            .GetSection(nameof(OpenTelemetrySettings))
            .Get<OpenTelemetrySettings>();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder
                .CreateDefault()
                .AddService(serviceName));

            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;

            options.AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = openTelemetrySettings!.Endpoint;
            });
        });


        var services = builder.Services;
        var otel = services.AddOpenTelemetry();

        otel.ConfigureResource(res => res.AddService(serviceName: serviceName));

        otel.WithMetrics(metrics =>
        {
            metrics
                .SetResourceBuilder((ResourceBuilder
                    .CreateDefault()
                    .AddService(serviceName))
                    .AddTelemetrySdk())
                .AddHttpClientInstrumentation();

            if (configureMetrics != null)
            {
                configureMetrics(metrics);
            }
        });

        otel.WithTracing(tracing =>
        {
            tracing
                .SetResourceBuilder((ResourceBuilder
                        .CreateDefault()
                        .AddService(serviceName))
                        .AddTelemetrySdk())
                .AddHttpClientInstrumentation(opt =>
                {
                    opt.RecordException = true;
                })
                .AddSqlClientInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.RecordException = true;
                })
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = openTelemetrySettings!.Endpoint;
                });

            if (configureTracing != null)
            {
                configureTracing(tracing);
            }
        });

        return services;
    }
}
