using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace SteadybitFailureInjection.Failures;

public class DelayFailure : ISteadybitFailure
{
  public int Priority => 1;

  public Task ExecuteAfterAsync(FunctionContext context, SteadybitFailureOptions options)
  {
    return Task.CompletedTask;
  }

  public async Task ExecuteBeforeAsync(FunctionContext context, SteadybitFailureOptions options)
  {
      if (!(options?.Delay != null &&
         options.Delay.MinimumLatencyValue.HasValue &&
         options.Delay.MaximumLatencyValue.HasValue &&
         options.Delay.RateValue.HasValue))
      {
        return;
      }

      int rate = options.Delay.RateValue.Value;
      if (rate <= 0 || rate > 100)
      {
        // _logger.LogError("Invalid rate value. It should be between 1 and 100.");
        return;
      }

      Random random = new Random();
      int randomValue = random.Next(1, 101);

      if (randomValue > rate)
      {
        return;
      }

      int minimumLatency = options.Delay.MinimumLatencyValue.Value;
      int maximumLatency = options.Delay.MaximumLatencyValue.Value;
      int delayRange = maximumLatency - minimumLatency;

      if (delayRange < 0)
      {
        // _logger.LogError("Invalid latency range. Maximum latency must be greater than or equal to minimum latency.");
        return;
      }

      double delay = (double)options.Delay.MinimumLatencyValue + (delayRange * new Random().NextDouble());
      await Task.Delay(TimeSpan.FromMilliseconds(delay));
  }
} 