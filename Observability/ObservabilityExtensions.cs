using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services
        , string serviceName
        , IConfiguration configuration
        , Action<MeterProviderBuilder>? configureMetrics = null
        , Action<TracerProviderBuilder>? configureTracing = null)
    {
        var otel = services.AddOpenTelemetry();

        otel.ConfigureResource(res => res.AddService(serviceName: serviceName));

        otel.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddPrometheusExporter();

            if (configureMetrics != null)
            {
                configureMetrics(metrics);
            }
        });

        otel.WithTracing(tracing =>
        {
            var tracingOtlpEndpoint = configuration["OTLP_ENDPOINT_URL"];

            tracing.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter();

            if (!string.IsNullOrWhiteSpace(tracingOtlpEndpoint))
            {
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                });
            }

            if (configureTracing != null)
            {
                configureTracing(tracing);
            }
        });

        return services;
    }
}
