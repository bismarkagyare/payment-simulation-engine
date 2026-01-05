namespace PaymentSimulation.Api.Domain.Payments;

public class Payment
{
    public Guid Id { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentMethod Method { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public static Payment Create(decimal amount, PaymentMethod method)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void MarkSucceeded()
    {
        Status = PaymentStatus.Suceeded;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = PaymentStatus.Failed;
        CompletedAtUtc = DateTime.UtcNow;
    }
}
