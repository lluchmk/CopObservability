namespace Messaging;
public class RabbitMqSettings
{
    public required string Hostname { get; init; }
    public required string QueueName { get; init; }
    public string ExchangeName { get; init; } = "";
}
