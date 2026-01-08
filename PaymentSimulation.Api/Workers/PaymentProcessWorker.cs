using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Domain.Payments;
using PaymentSimulation.Api.Infra.Queue;

namespace PaymentSimulation.Api.Workers;

public class PaymentProcessWorker : BackgroundService
{
    private readonly InMemoryQueue _queue;

    private readonly IPaymentRepository _paymentRepository;

    public PaymentProcessWorker(InMemoryQueue queue, IPaymentRepository paymentRepository)
    {
        _queue = queue;
        _paymentRepository = paymentRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //if there are no payments to process
            if (!_queue.TryDequeue(out var paymentId))
            {
                //no work right now, wait a bit
                await Task.Delay(500);
                continue;
            }

            Payment? payment = _paymentRepository.GetById(paymentId);

            if (payment == null)
                continue;

            payment.MarkProcessing();
            _paymentRepository.Update(payment);

            await Task.Delay(2000, stoppingToken);

            // Simulate success/failure
            var success = Random.Shared.Next(0, 100) > 30;

            if (success)
                payment.MarkSucceeded();
            else
                payment.MarkFailed();

            _paymentRepository.Update(payment);
        }
    }
}
