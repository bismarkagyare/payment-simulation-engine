using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Config;
using PaymentSimulation.Api.Domain.Payments;
using PaymentSimulation.Api.Infra.Queue;

namespace PaymentSimulation.Api.Workers;

public class WebhookWorker : BackgroundService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly WebhookOptions _options;
    private readonly HttpClient _http;
    private readonly ILogger<WebhookWorker> _logger;
    private InMemoryQueue? _webhookQueue;

    public WebhookWorker(
        IPaymentRepository paymentRepository,
        IOptions<WebhookOptions> options,
        ILogger<WebhookWorker> logger
    )
    {
        _paymentRepository = paymentRepository;
        _options = options.Value;
        _logger = logger;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public void SetWebhookQueue(InMemoryQueue queue)
    {
        _webhookQueue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_webhookQueue != null && _webhookQueue.TryDequeue(out var paymentId))
            {
                var payment = _paymentRepository.GetById(paymentId);

                if (payment == null)
                    continue;

                await SendWebhookAsync(payment, stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SendWebhookAsync(Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                @event = $"payment.{payment.Status.ToString().ToLower()}",
                timestamp = DateTime.UtcNow,
                data = new
                {
                    id = payment.Id,
                    amount = payment.Amount,
                    method = payment.Method,
                    status = payment.Status,
                },
            };

            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload, jsonOptions),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation(
                "Sending webhook for payment {PaymentId} (status: {Status}) to {Url}",
                payment.Id,
                payment.Status,
                _options.Url
            );

            var response = await _http.PostAsync(_options.Url, content, cancellationToken);

            _logger.LogInformation(
                "Webhook response for payment {PaymentId}: {StatusCode}",
                payment.Id,
                response.StatusCode
            );

            if (response.IsSuccessStatusCode)
            {
                payment.MarkWebhookSent();
                _paymentRepository.Update(payment);
                _logger.LogInformation("Webhook delivered for payment {PaymentId}", payment.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Webhook failed for payment {PaymentId} with status {StatusCode}",
                    payment.Id,
                    response.StatusCode
                );
                HandleWebhookFailure(payment);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Webhook HTTP error for payment {PaymentId}: {Message}",
                payment.Id,
                ex.Message
            );
            HandleWebhookFailure(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected webhook error for payment {PaymentId}", payment.Id);
            HandleWebhookFailure(payment);
        }
    }

    private void HandleWebhookFailure(Payment payment)
    {
        if (payment.CanRetry())
        {
            payment.IncrementRetry();
            payment.MarkWebhookFailed();
            _paymentRepository.Update(payment);

            _logger.LogWarning(
                "Webhook retry {Count}/{Max} for payment {PaymentId}",
                payment.RetryCount,
                payment.MaxRetries,
                payment.Id
            );

            _webhookQueue?.Enqueue(payment.Id);
        }
        else
        {
            payment.MarkWebhookDead();
            _paymentRepository.Update(payment);
            _logger.LogError("Webhook dead-letter for payment {PaymentId}", payment.Id);
        }
    }
}
