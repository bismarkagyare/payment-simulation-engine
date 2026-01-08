using System.Collections.Concurrent;

namespace PaymentSimulation.Api.Infra.Queue;

public class InMemoryQueue
{
    private readonly ConcurrentQueue<Guid> _queue = new();

    public void Enqueue(Guid paymentId)
    {
        _queue.Enqueue(paymentId);
    }

    public bool TryDequeue(out Guid paymentId)
    {
        return _queue.TryDequeue(out paymentId);
    }
}
