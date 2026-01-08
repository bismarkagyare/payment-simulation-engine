using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Domain.Payments;

namespace PaymentSimulation.Api.Infra.Persistence;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly Dictionary<Guid, Payment> _payments = new();

    public void Add(Payment payment)
    {
        _payments[payment.Id] = payment;
    }

    public Payment? GetById(Guid id)
    {
        _payments.TryGetValue(id, out var payment);
        return payment;
    }

    public void Update(Payment payment)
    {
        _payments[payment.Id] = payment;
    }
}
