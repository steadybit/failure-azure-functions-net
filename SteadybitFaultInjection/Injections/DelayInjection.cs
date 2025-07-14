using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjections.Injections;

public class DelayFailure : ISteadybitInjection
{
    private readonly ILogger<DelayFailure> _logger;

    public DelayFailure(ILogger<DelayFailure> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }

    public async Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        if (options?.Delay == null)
        {
            _logger.LogWarning(
                "Keys Steadybit:Injection:Delay:MinimumLatency and Steadybit:Injection:Delay:MaximumLatency are not provided, skipping injection..."
            );
            return;
        }

        if (!options.Delay.MinimumLatencyValue.HasValue)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:Delay:MinimumLatency is not provided or invalid, skipping injection..."
            );
            return;
        }

        if (!options.Delay.MaximumLatencyValue.HasValue)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:Delay:MaximumLatency is not provided or invalid, skipping injection..."
            );
            return;
        }

        if (options.Delay.MaximumLatencyValue < options.Delay.MinimumLatencyValue)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:Delay:MaximumLatency must be greater than or equal to Steadybit:Injection:Delay:MinimumLatency, skipping injection..."
            );
            return;
        }

        int minimumLatency = options.Delay.MinimumLatencyValue.Value;
        int maximumLatency = options.Delay.MaximumLatencyValue.Value;
        int delayRange = maximumLatency - minimumLatency;

        double delay =
            (double)options.Delay.MinimumLatencyValue + (delayRange * new Random().NextDouble());

        _logger.LogInformation(
            $"Injecting delay of {delay} milliseconds. Range: {minimumLatency} - {maximumLatency} milliseconds."
        );
        await Task.Delay(TimeSpan.FromMilliseconds(delay));
    }
}
