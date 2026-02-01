using Microsoft.Extensions.Options;
using PaymentSimulation.Api.Config;
using RabbitMQ.Client;

namespace PaymentSimulation.Api.Messaging;

//create rabbitmq exchanges, queues, and bindings at startup
//you can only create queues/exchanges through a channel, and you can only get a channel from a connection
public class RabbitMQTopologySetup : IRabbitMQTopologySetup
{
    private readonly IRabbitMQConnection _connection;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQTopologySetup> _logger;

    public RabbitMQTopologySetup(
        IRabbitMQConnection connection,
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQTopologySetup> logger
    )
    {
        _connection = connection;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SetupAsync()
    {
        using var channel = _connection.CreateChannel();

        await channel.ExchangeDeclareAsync(
            exchange: $"{_options.ExchangeName}.dlx",
            type: ExchangeType.Direct,
            durable: true
        );

        await channel.QueueDeclareAsync(
            queue: _options.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        //binding: connect DLX to the DLQ
        await channel.QueueBindAsync(
            queue: _options.DeadLetterQueueName,
            exchange: $"{_options.ExchangeName}.dlx",
            routingKey: "dead-letter"
        );

        _logger.LogInformation(
            "Created dead-letter exchange and queue: {Queue}",
            _options.DeadLetterQueueName
        );

        //create main exchange
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true
        );

        _logger.LogInformation("Created exchange: {Exchange} (type: topic)", _options.ExchangeName);

        //create payment processing queue
        //this replaces the InMemoryQueue in PaymentService
        //messages published w/ routing key "payment.created" lands here

        var processQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", $"{_options.ExchangeName}.dlx" },
            { "x-dead-letter-routing-key", "dead-letter" },
        };

        await channel.QueueDeclareAsync(
            queue: _options.ProcessQueueName, //e.g., "payment.process"
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: processQueueArgs
        );

        //binding for process queue
        await channel.QueueBindAsync(
            queue: _options.ProcessQueueName,
            exchange: _options.ExchangeName,
            routingKey: "payment.created"
        );

        _logger.LogInformation(
            "Created queue: {Queue} bound to {Exchange} with routing key 'payment.created'",
            _options.ProcessQueueName,
            _options.ExchangeName
        );

        //create webhook delivery queue
        var webhookQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", $"{_options.ExchangeName}.dlx" },
            { "x-dead-letter-routing-key", "dead-letter" },
        };

        await channel.QueueDeclareAsync(
            queue: _options.WebhookQueueName, // e.g., "payment.webhook"
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: webhookQueueArgs
        );

        await channel.QueueBindAsync(
            queue: _options.WebhookQueueName,
            exchange: _options.ExchangeName,
            routingKey: "payment.processed"
        );

        _logger.LogInformation(
            "Created queue: {Queue} bound to {Exchange} with routing key 'payment.processed'",
            _options.WebhookQueueName,
            _options.ExchangeName
        );

        _logger.LogInformation("RabbitMQ topology setup complete");
    }
}
