using Microsoft.Extensions.Logging;

namespace SteadybitFaultInjection.Injections;

public class DelayInjection(ILogger<DelayInjection> logger) : ISteadybitInjection
{
    private readonly ILogger<DelayInjection> _logger = logger;

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
            "Injecting delay of {Delay} milliseconds. Range: {MinimumLatency} - {MaximumLatency} milliseconds.",
            delay, minimumLatency, maximumLatency
        );
        await Task.Delay(TimeSpan.FromMilliseconds(delay));
    }
}
