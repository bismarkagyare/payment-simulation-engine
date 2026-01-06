using Microsoft.AspNetCore.Mvc;
using PaymentSimulation.Api.Application.Payments;
using PaymentSimulation.Api.Domain.Payments;
using PaymentSimulation.Api.Dtos.Requests;
using PaymentSimulation.Api.Dtos.Responses;

namespace PaymentSimulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public ActionResult<PaymentResponse> CreatePayment(CreatePaymentRequest request)
    {
        var payment = _paymentService.CreatePayment(request.Amount, request.Method);
        return Ok(ToResponse(payment));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<PaymentResponse> GetPayment(Guid id)
    {
        var payment = _paymentService.GetPayment(id);

        if (payment == null)
            return NotFound();

        return Ok(ToResponse(payment));
    }

    private static PaymentResponse ToResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Method = payment.Method,
            Status = payment.Status,
        };
    }
}
