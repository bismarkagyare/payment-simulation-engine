namespace PaymentSimulation.Api.Messaging;

public interface IRabbitMQTopologySetup
{
    Task SetupAsync();
}
