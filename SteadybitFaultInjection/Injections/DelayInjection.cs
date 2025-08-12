using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection.Injections;

public class DelayInjection : ISteadybitInjection
{
    private readonly ILogger<DelayInjection> _logger;

    public DelayInjection(ILogger<DelayInjection> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAfterAsync(ISteadybitContext context, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }

    public async Task ExecuteBeforeAsync(
        ISteadybitContext context,
        SteadybitInjectionOptions options
    )
    {
        if (options?.Delay == null)
        {
            _logger.LogWarning("Delay options are not provided, skipping injection...");
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
