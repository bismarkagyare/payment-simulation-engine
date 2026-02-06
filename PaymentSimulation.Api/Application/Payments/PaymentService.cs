using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Domain.Payments;
using PaymentSimulation.Api.Infra.Queue;
using PaymentSimulation.Api.Messaging.Contracts;
using PaymentSimulation.Api.Messaging.Interfaces;

namespace PaymentSimulation.Api.Application.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    //private readonly InMemoryQueue _queue;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentRepository paymentRepository, IMessagePublisher publisher)
    {
        _paymentRepository = paymentRepository;
        _publisher = publisher;
    }

    public Payment CreatePayment(long amount, PaymentMethod method)
    {
        var payment = Payment.Create(amount, method);

        _paymentRepository.Add(payment);

        var message = new PaymentCreatedMessage
        {
            PaymentId = payment.Id,
            Amount = payment.Amount,
            Method = method.ToString(),
            CreatedAtUtc = payment.CreatedAtUtc,
            RetryCount = 0, // First attempt
        };

        _ = _publisher.PublishAsync("payment.created", message);

        _logger.LogInformation(
            "Payment {PaymentId} created and published for processing. "
                + "Amount: {Amount}, Method: {Method}",
            payment.Id,
            payment.Amount,
            payment.Method
        );

        return payment;
    }

    public Payment? GetPayment(Guid id)
    {
        return _paymentRepository.GetById(id);
    }

    // private void ProcessPayment(Payment payment)
    // {
    //     // 80% chance of success
    //     var isSuccessful = _random.Next(1, 101) <= 80;

    //     if (isSuccessful)
    //     {
    //         payment.MarkSucceeded();
    //     }
    //     else
    //     {
    //         payment.MarkFailed();
    //     }
    // }
}
