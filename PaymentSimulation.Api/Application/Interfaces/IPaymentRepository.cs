using PaymentSimulation.Api.Domain.Payments;

namespace PaymentSimulation.Api.Application.Interfaces;

public interface IPaymentRepository
{
    void Add(Payment payment);

    void Update(Payment payment);

    Payment? GetById(Guid id);
}
