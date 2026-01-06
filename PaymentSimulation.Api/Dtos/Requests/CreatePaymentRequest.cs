using PaymentSimulation.Api.Domain.Payments;

namespace PaymentSimulation.Api.Dtos.Requests;

public class CreatePaymentRequest
{
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }
}
