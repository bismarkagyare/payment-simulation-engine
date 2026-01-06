using PaymentSimulation.Api.Domain.Payments;

namespace PaymentSimulation.Api.Dtos.Responses;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
}
