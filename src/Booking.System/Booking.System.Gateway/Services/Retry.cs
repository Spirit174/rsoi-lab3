using System.Collections.Concurrent;

namespace Booking.System.Gateway.Services;

public class RetryItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string OperationType { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public Func<Task<bool>> Action { get; set; } = () => Task.FromResult(false);
}

public interface IRetryQueue
{
    void Enqueue(RetryItem item);
    Task ProcessQueueAsync(CancellationToken cancellationToken = default);
}

public class RetryQueue : IRetryQueue, IHostedService
{
    private readonly ConcurrentQueue<RetryItem> _queue = new();
    private readonly ILogger<RetryQueue> _logger;
    private Timer? _timer;

    public RetryQueue(ILogger<RetryQueue> logger)
    {
        _logger = logger;
    }

    public void Enqueue(RetryItem item)
    {
        _queue.Enqueue(item);
        _logger.LogInformation("Item enqueued for retry: {OperationType} for user {Username}", 
            item.OperationType, item.Username);
    }

    public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
    {
        var itemsToRetry = new List<RetryItem>();
        
        while (_queue.TryDequeue(out var item))
        {
            if (item.RetryCount >= 3 || DateTime.UtcNow - item.CreatedAt > TimeSpan.FromSeconds(10))
            {
                _logger.LogWarning("Retry item expired or max retries exceeded: {Id}", item.Id);
                continue;
            }

            itemsToRetry.Add(item);
        }

        foreach (var item in itemsToRetry)
        {
            try
            {
                var success = await item.Action();
                if (success)
                {
                    _logger.LogInformation("Retry operation completed successfully: {Id}", item.Id);
                }
                else
                {
                    item.RetryCount++;
                    _queue.Enqueue(item);
                    _logger.LogWarning("Retry operation failed, requeuing: {Id}, attempt: {Attempt}", 
                        item.Id, item.RetryCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Retry operation failed: {Id}", item.Id);
                item.RetryCount++;
                _queue.Enqueue(item);
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(async void (_) => await ProcessQueueAsync(), null, 
            TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}