using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Config;
using PaymentSimulation.Api.Domain.Payments;
using PaymentSimulation.Api.Infra.Queue;

namespace PaymentSimulation.Api.Workers;

//worker that sends webhooks for completed payments
public class WebhookWorker : BackgroundService
{
    private readonly InMemoryQueue _queue;

    private readonly IPaymentRepository _paymentRepository;

    private readonly WebhookOptions _options;

    private readonly HttpClient _http = new();

    public WebhookWorker(
        InMemoryQueue queue,
        IPaymentRepository paymentRepository,
        IOptions<WebhookOptions> options
    )
    {
        _queue = queue;
        _paymentRepository = paymentRepository;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var paymentId))
            {
                var payment = _paymentRepository.GetById(paymentId);

                if (payment == null)
                    continue;

                await SendWebhookAsync(payment);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SendWebhookAsync(Payment payment)
    {
        var payload = new
        {
            @event = $"payment.{payment.Status.ToString().ToLower()}",

            data = new
            {
                id = payment.Id,
                amount = payment.Amount,
                method = payment.Method,
                status = payment.Status,
            },
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _http.PostAsync(_options.Url, content);
            if (response.IsSuccessStatusCode)
            {
                payment.MarkWebhookSent();
            }
            else
            {
                payment.MarkWebhookFailed();
                //optionally retry later
            }
        }
        catch
        {
            payment.MarkWebhookFailed();
        }
    }
}
