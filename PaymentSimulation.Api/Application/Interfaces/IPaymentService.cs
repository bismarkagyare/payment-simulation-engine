using PaymentSimulation.Api.Domain.Payments;

namespace PaymentSimulation.Api.Application.Interfaces;

public interface IPaymentService
{
    Payment CreatePayment(long amount, PaymentMethod method);
    Payment? GetPayment(Guid id);
}
