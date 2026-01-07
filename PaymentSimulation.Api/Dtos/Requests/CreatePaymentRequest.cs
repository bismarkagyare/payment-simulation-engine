using System.ComponentModel.DataAnnotations;
using PaymentSimulation.Api.Domain.Payments;

namespace PaymentSimulation.Api.Dtos.Requests;

public class CreatePaymentRequest
{
    [Required]
    [Range(1, long.MaxValue)]
    public long Amount { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }
}
