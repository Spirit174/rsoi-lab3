using System.Collections.Concurrent;
using Booking.System.Gateway.DTO;

namespace Booking.System.Gateway.CircuitBreaker;

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}

public class CircuitBreaker
{
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _states = new();
    private readonly ConcurrentDictionary<string, int> _failureCounts = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastFailureTimes = new();
    private readonly ConcurrentDictionary<string, Func<Task<bool>>> _healthChecks = new();
    private readonly ILogger<CircuitBreaker> _logger;
    
    private const int MAX_FAILURES = 5;
    private readonly TimeSpan OPEN_TIMEOUT = TimeSpan.FromSeconds(30);

    public CircuitBreaker(ILogger<CircuitBreaker> logger)
    {
        _logger = logger;
    }

    public void RegisterHealthCheck(string serviceName, Func<Task<bool>> healthCheck)
    {
        _healthChecks[serviceName] = healthCheck;
    }

    public async Task<T> ExecuteAsync<T>(string serviceName, Func<Task<T>> action, Func<T> fallback)
    {
        var state = GetState(serviceName);

        if (state == CircuitBreakerState.Open)
        {
            _logger.LogWarning("Circuit breaker OPEN for {ServiceName}, using fallback", serviceName);
            return fallback();
        }

        try
        {
            var result = await action();
            Reset(serviceName);
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure(serviceName, ex);
            return fallback();
        }
    }

    public async Task<ServiceResponse<T>> ExecuteAsync<T>(
        string serviceName, 
        Func<Task<ServiceResponse<T>>> action, 
        ServiceResponse<T> fallback)
    {
        var state = GetState(serviceName);

        if (state == CircuitBreakerState.Open)
        {
            _logger.LogWarning("Circuit breaker OPEN for {ServiceName}, using fallback", serviceName);
            return fallback;
        }

        try
        {
            var result = await action();
            
            if (result.IsSuccess)
            {
                Reset(serviceName);
                return result;
            }
            else
            {
                RecordFailure(serviceName, new Exception($"Service returned error: {result.StatusCode}"));
                return fallback;
            }
        }
        catch (Exception ex)
        {
            RecordFailure(serviceName, ex);
            return fallback;
        }
    }

    private CircuitBreakerState GetState(string serviceName)
    {
        if (!_states.TryGetValue(serviceName, out var state))
        {
            state = CircuitBreakerState.Closed;
            _states[serviceName] = state;
        }

        if (state == CircuitBreakerState.Open && 
            _lastFailureTimes.TryGetValue(serviceName, out var lastFailureTime) &&
            DateTime.UtcNow - lastFailureTime > OPEN_TIMEOUT)
        {
            // Переходим в HalfOpen состояние для тестирования
            state = CircuitBreakerState.HalfOpen;
            _states[serviceName] = state;
            _ = TestServiceRecovery(serviceName);
        }

        return state;
    }

    private void RecordFailure(string serviceName, Exception ex)
    {
        var failureCount = _failureCounts.AddOrUpdate(serviceName, 1, (_, count) => count + 1);
        _lastFailureTimes[serviceName] = DateTime.UtcNow;

        _logger.LogWarning(ex, "Failure recorded for {ServiceName}. Failure count: {Count}", serviceName, failureCount);

        if (failureCount >= MAX_FAILURES)
        {
            _states[serviceName] = CircuitBreakerState.Open;
            _logger.LogError("Circuit breaker OPENED for {ServiceName}", serviceName);
        }
    }

    private void Reset(string serviceName)
    {
        _failureCounts[serviceName] = 0;
        _states[serviceName] = CircuitBreakerState.Closed;
        _logger.LogInformation("Circuit breaker RESET for {ServiceName}", serviceName);
    }

    private async Task TestServiceRecovery(string serviceName)
    {
        if (_healthChecks.TryGetValue(serviceName, out var healthCheck))
        {
            try
            {
                var isHealthy = await healthCheck();
                if (isHealthy)
                {
                    Reset(serviceName);
                    _logger.LogInformation("Service {ServiceName} recovered, circuit breaker CLOSED", serviceName);
                }
                else
                {
                    _states[serviceName] = CircuitBreakerState.Open;
                    _logger.LogWarning("Service {ServiceName} still unhealthy, circuit breaker remains OPEN", serviceName);
                }
            }
            catch
            {
                _states[serviceName] = CircuitBreakerState.Open;
            }
        }
    }
}