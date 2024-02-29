using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Observability.Asp;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddAspObservability(this IHostApplicationBuilder builder
        , Action<MeterProviderBuilder>? configureMetrics = null
        , Action<TracerProviderBuilder>? configureTracing = null)
    {
        builder.AddObservability(configureMetrics: metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddPrometheusExporter();

            if (configureMetrics != null)
            {
                configureMetrics(metrics);
            }
        },
        configureTracing: tracing =>
        {
            tracing.AddAspNetCoreInstrumentation(options =>
            {
                options.Filter = (req) => !req.Request.Path.ToUriComponent().Equals("/metrics", StringComparison.OrdinalIgnoreCase);
            });

            if (configureTracing != null)
            {
                configureTracing(tracing);
            }
        });

        return builder.Services;
    }

    public static IApplicationBuilder UseTracingExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()!.Error;
                var activity = Activity.Current;

                activity?.RecordException(exception);
                activity?.SetStatus(Status.Error.WithDescription(exception.Message));

                await Results.Problem().ExecuteAsync(context);
            });
        });

        return app;
    }
}
