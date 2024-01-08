using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Messaging;

public class MessageSender : IDisposable
{
    private static readonly ActivitySource ActivitySource = new(nameof(MessageSender));
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly ILogger<MessageSender> _logger;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public MessageSender(ILogger<MessageSender> logger, IOptions<RabbitMqSettings> settings)
    {
        _logger = logger;
        _rabbitMqSettings = settings.Value;

        var factory = new ConnectionFactory { HostName = _rabbitMqSettings.Hostname };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare(_rabbitMqSettings.QueueName, false, false);

        _connection = connection;
        _channel = channel;
    }

    public void SendMessage<T>(T message)
    {
        try
        {
            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#span-name
            var activityName = $"{_rabbitMqSettings.QueueName} send";
            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer);

            var props = _channel.CreateBasicProperties();

            // Depending on Sampling (and whether a listener is registered or not), the
            // activity above may not be created.
            // If it is created, then propagate its context.
            // If it is not created, the propagate the Current context,
            // if any.
            ActivityContext contextToInject = default;
            if (activity != null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            // Inject the ActivityContext into the message headers to propagate trace context to the receiving service.
            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), props, InjectTraceContextIntoBasicProperties);

            var serializedMessage = JsonSerializer.Serialize(message);
            
            var body = Encoding.UTF8.GetBytes(serializedMessage);
            _channel.BasicPublish(exchange: _rabbitMqSettings.ExchangeName,
                routingKey: _rabbitMqSettings.QueueName,
                basicProperties: props,
                body: body);

            _logger.MessageSent(serializedMessage);
        }
        catch (Exception ex)
        {
            _logger.MessageException(ex);
            throw;
        }
    }

    private void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
    {
        try
        {
            if (props.Headers is null)
            {
                props.Headers = new Dictionary<string, object>();
            }

            props.Headers[key] = value;
        }
        catch (Exception ex)
        {
            _logger.ContextInjectException(ex);
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
