using RabbitMQ.Client;

namespace PaymentSimulation.Api.Messaging;

public interface IRabbitMQConnection : IDisposable
{
    //creates a new channel for publishing or consuming messages
    IChannel CreateChannel();

    bool IsConnected { get; }
}
