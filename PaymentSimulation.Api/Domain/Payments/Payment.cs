namespace PaymentSimulation.Api.Domain.Payments;

public class Payment
{
    public Guid Id { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentMethod Method { get; private set; }

    public PaymentStatus Status { get; private set; }

    public int RetryCount { get; private set; } = 0;

    public int MaxRetries { get; private set; } = 3;

    public WebhookStatus WebhookStatus { get; private set; } = WebhookStatus.Pending;

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

    public void MarkProcessing()
    {
        if (Status != PaymentStatus.Pending)
        {
            return;
        }
        Status = PaymentStatus.Processing;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void MarkSucceeded()
    {
        if (Status != PaymentStatus.Processing)
        {
            return;
        }
        Status = PaymentStatus.Succeeded;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status != PaymentStatus.Processing)
        {
            return;
        }
        Status = PaymentStatus.Failed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void IncrementRetry()
    {
        RetryCount++;
    }

    public bool CanRetry()
    {
        return RetryCount < MaxRetries;
    }

    public void MarkWebhookSent()
    {
        WebhookStatus = WebhookStatus.Suceeded;
    }

    public void MarkWebhookFailed()
    {
        WebhookStatus = WebhookStatus.Failed;
    }

    public void MarkWebhookDead()
    {
        WebhookStatus = WebhookStatus.Dead;
    }
}
