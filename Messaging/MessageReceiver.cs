using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Messaging;

public class MessageReceiver : IDisposable
{
    private static readonly ActivitySource ActivitySource = new(nameof(MessageReceiver));
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly ILogger<MessageReceiver> _logger;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public MessageReceiver(ILogger<MessageReceiver> logger, IOptions<RabbitMqSettings> settings)
    {
        _logger = logger;
        _rabbitMqSettings = settings.Value;

        var factory = new ConnectionFactory
        { 
            HostName = _rabbitMqSettings.Hostname,
            DispatchConsumersAsync = true
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare(_rabbitMqSettings.QueueName, false, false);

        _connection = connection;
        _channel = channel;
    }

    public void StartConsumer<T>(Func<T, Task> processMessage)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, eventArgs) =>
        {
            await ReceiveMessage(eventArgs, processMessage);
        };
        _channel.BasicConsume(queue: _rabbitMqSettings.QueueName, autoAck: true, consumer);
    }

    private async Task ReceiveMessage<T>(BasicDeliverEventArgs eventArgs, Func<T, Task> processMessage)
    {
        // Extract the PropagationContext of the upstrea parent from the message headers.
        var parentContext = Propagator.Extract(default, eventArgs.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#span-name
        var activityName = $"{_rabbitMqSettings.QueueName} message receive";
        using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);

        try
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var request = JsonSerializer.Deserialize<T>(message);

            if (request is null)
            {
                _logger.InvalidMessageReceived(message);
                return;
            }

            _logger.MessageReceived(message);
            await processMessage(request);
        }
        catch (Exception ex)
        {
            _logger.ProcessMessageException(ex);
            // Record the exception into the trace and mark it as failed
            activity?.RecordException(ex);
            activity?.SetStatus(Status.Error.WithDescription(ex.Message));
        }
    }

    private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
    {
        try
        {
            if (props.Headers.TryGetValue(key, out var value))
            {
                var bytes = (byte[])value;
                return new[] { Encoding.UTF8.GetString(bytes) };
            }
        }
        catch (Exception ex)
        {
            _logger.ContextRetrieveException(ex);
        }

        return Enumerable.Empty<string>();
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
