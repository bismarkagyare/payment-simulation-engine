using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Domain.Payments;
using PaymentSimulation.Api.Infra.Queue;

namespace PaymentSimulation.Api.Workers;

public class PaymentProcessWorker : BackgroundService
{
    private readonly InMemoryQueue _processQueue;
    private readonly InMemoryQueue _webhookQueue;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentProcessWorker> _logger;

    public PaymentProcessWorker(
        InMemoryQueue processQueue,
        IPaymentRepository paymentRepository,
        ILogger<PaymentProcessWorker> logger
    )
    {
        _processQueue = processQueue;
        _paymentRepository = paymentRepository;
        _logger = logger;
        _webhookQueue = new InMemoryQueue();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PaymentProcessWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            //if there are no payments to process
            if (!_processQueue.TryDequeue(out var paymentId))
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
            _logger.LogInformation("Payment {PaymentId} marked as Processing", paymentId);

            await Task.Delay(2000, stoppingToken);

            //simulate success/failure
            var success = Random.Shared.Next(0, 100) > 30;

            if (success)
            {
                payment.MarkSucceeded();
                _logger.LogInformation("Payment {PaymentId} succeeded", paymentId);
            }
            else
            {
                payment.MarkFailed();
                _logger.LogWarning("Payment {PaymentId} failed", paymentId);
            }

            _paymentRepository.Update(payment);

            //enqueue for webhook delivery
            _webhookQueue.Enqueue(paymentId);
            _logger.LogInformation("Payment {PaymentId} enqueued for webhook delivery", paymentId);

            //retry failed payments
            if (payment.Status == PaymentStatus.Failed && payment.CanRetry())
            {
                payment.IncrementRetry();
                _paymentRepository.Update(payment);
                _processQueue.Enqueue(paymentId);
                _logger.LogWarning(
                    "Payment {PaymentId} re-enqueued for retry ({Count}/{Max})",
                    paymentId,
                    payment.RetryCount,
                    payment.MaxRetries
                );
            }
        }
    }

    //public property so WebhookWorker can access this queue
    public InMemoryQueue WebhookQueue => _webhookQueue;
}
