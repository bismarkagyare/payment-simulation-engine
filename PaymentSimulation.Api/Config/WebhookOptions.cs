namespace PaymentSimulation.Api.Config;

public class WebhookOptions
{
    public string Url { get; set; } = "http://localhost:8080/webhooks/payments";

    public int MaxRetries { get; set; } = 3;
}
