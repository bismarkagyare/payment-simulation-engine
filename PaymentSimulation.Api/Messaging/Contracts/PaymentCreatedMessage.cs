namespace PaymentSimulation.Api.Messaging.Contracts;

public record PaymentCreatedMessage
{
    public Guid PaymentId { get; init; }

    public decimal Amount { get; init; }

    public string Method { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }

    public int RetryCount { get; init; }
}
