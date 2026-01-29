using Microsoft.Extensions.Options;
using PaymentSimulation.Api.Config;
using RabbitMQ.Client;

namespace PaymentSimulation.Api.Messaging;

//create tcp connectin and channels
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

    public IChannel CreateChannel()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQConnection));

        EnsureConnected();

        return _connection!.CreateChannelAsync().GetAwaiter().GetResult();
    }

    private void EnsureConnected()
    {
        if (IsConnected)
            return;

        lock (_lock)
        {
            if (IsConnected)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            };

            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _logger.LogInformation(
                    "Successfully connected to RabbitMQ at {Host}:{Port}",
                    _options.HostName,
                    _options.Port
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to connect to RabbitMQ at {Host}:{Port}",
                    _options.HostName,
                    _options.Port
                );
                throw;
            }
        }
    }

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
