using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Domain.Payments;

namespace PaymentSimulation.Api.Application.Payments;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly Random _random = new();

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public Payment CreatePayment(decimal amount, PaymentMethod method)
    {
        var payment = Payment.Create(amount, method);

        ProcessPayment(payment);

        _paymentRepository.Add(payment);

        return payment;
    }

    public Payment? GetPayment(Guid id)
    {
        return _paymentRepository.GetById(id);
    }

    private void ProcessPayment(Payment payment)
    {
        // 80% chance of success
        var isSuccessful = _random.Next(1, 101) <= 80;

        if (isSuccessful)
        {
            payment.MarkSucceeded();
        }
        else
        {
            payment.MarkFailed();
        }
    }
}
