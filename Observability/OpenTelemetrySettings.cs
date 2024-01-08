namespace Observability;

public class OpenTelemetrySettings
{
    public required Uri Endpoint { get; init; }
    public required Uri JaegerEndpoint { get; init; }
}
