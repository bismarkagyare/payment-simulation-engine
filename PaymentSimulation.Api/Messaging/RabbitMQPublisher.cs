using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentSimulation.Api.Config;
using PaymentSimulation.Api.Messaging.Interfaces;
using RabbitMQ.Client;

namespace PaymentSimulation.Api.Messaging;

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQPublisher(
        IRabbitMQConnection connection,
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQPublisher> logger
    )
    {
        _connection = connection;
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public async Task PublishAsync<T>(
        string routingKey,
        T message,
        CancellationToken cancellationToken = default
    )
    {
        using var channel = _connection.CreateChannel();

        //serialise the msg to json bytes
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        //metadata added to the message
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
        };

        //publish the msg
        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Published message to exchange '{Exchange}' with routing key '{RoutingKey}'. "
                + "MessageId: {MessageId}, Type: {MessageType}",
            _options.ExchangeName,
            routingKey,
            properties.MessageId,
            typeof(T).Name
        );
    }
}
