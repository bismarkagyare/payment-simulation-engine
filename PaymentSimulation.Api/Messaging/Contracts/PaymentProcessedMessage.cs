namespace PaymentSimulation.Api.Messaging.Contracts;

public record PaymentProcessedMessage
{
    public Guid PaymentId { get; init; }

    public string Status { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Method { get; init; } = string.Empty;

    public DateTime ProcessedAtUtc { get; init; }

    public int RetryCount { get; init; } = 0;
}
