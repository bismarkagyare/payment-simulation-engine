namespace PaymentSimulation.Api.Config;

public class RabbitMQOptions
{
    public const string SectionName = "RabbitMQ";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    //exchange is the rabbitmq routing engine:decide which queue should receive a msg
    public string ExchangeName { get; set; } = "payments.exchange";
    public string ProcessQueueName { get; set; } = "payment.process";
    public string WebhookQueueName { get; set; } = "payment.webhook";
    public string DeadLetterQueueName { get; set; } = "payment.dlq";
    public int MaxRetryAttempts { get; set; } = 3;
    public int InitialRetryDelayMs { get; set; } = 1000;
}
