using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Domain.Payments;
using PaymentSimulation.Api.Infra.Queue;

namespace PaymentSimulation.Api.Application.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    private readonly InMemoryQueue _queue;

    public PaymentService(IPaymentRepository paymentRepository, InMemoryQueue queue)
    {
        _paymentRepository = paymentRepository;
        _queue = queue;
    }

    public Payment CreatePayment(long amount, PaymentMethod method)
    {
        var payment = Payment.Create(amount, method);

        _paymentRepository.Add(payment);

        _queue.Enqueue(payment.Id);

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
