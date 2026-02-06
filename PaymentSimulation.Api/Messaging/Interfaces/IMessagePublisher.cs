namespace PaymentSimulation.Api.Messaging.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        string routingKey,
        T message,
        CancellationToken cancellationToken = default
    );
}
