using Microsoft.Extensions.Options;
using PaymentSimulation.Api.Config;
using RabbitMQ.Client;

namespace PaymentSimulation.Api.Messaging;

public class RabbitMQConnection : IRabbitMQConnection
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQConnection> _logger;
    private IConnection? _connection;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMQConnection(IOptions<RabbitMQOptions> options, ILogger<RabbitMQConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConnected => _connection?.IsOpen ?? false;

    public IChannel CreateChannel() { }

    public void Dispose()
    {
        //prevent double disposal
        if (_disposed)
            return;
        _disposed = true;

        //close the connection(close all channels)
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
