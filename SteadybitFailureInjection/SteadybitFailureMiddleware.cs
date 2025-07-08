using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace SteadybitFailureInjection;

public class SteadybitFailureMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IConfiguration _configuration;
  private readonly IFeatureManager _featureManager;
  private readonly ILogger _logger;

  public SteadybitFailureMiddleware(RequestDelegate next, IConfiguration configuration, IFeatureManager featureManager, ILoggerFactory? loggerFactory)
  {
    _next = next;
    _configuration = configuration;
    _featureManager = featureManager;
    _logger = loggerFactory?.CreateLogger<SteadybitFailureMiddleware>() ?? NullLoggerFactory.Instance.CreateLogger<SteadybitFailureMiddleware>();
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var options = new SteadybitFailureOptions();
    IConfigurationSection section = _configuration.GetSection("Steadybit:FaultInjection");

    if (section == null)
    {
      _logger.LogError("Steadybit Fault Injection configuration section not found.");
      await _next(context);
      return;
    }

    section.Bind(options);

    bool isFeatureEnabled = await _featureManager.IsEnabledAsync("SteadybitFaultInjectionEnabled");

    if (!isFeatureEnabled)
    {
      await _next(context);
      return;
    }

    if (options?.Exception != null && options?.Exception.Message != null)
    {
      throw new Exception(options.Exception.Message);
    }

    if (options?.Revision == null)
    {
      _logger.LogWarning("Sentinel key Steadybit:FaultInjection:Revision is not set. Configuration won't be able to refresh.");
      await _next(context);
      return;
    }

    if (options?.Delay != null && options.Delay.MinimumLatencyValue != null && options.Delay.MaximumLatencyValue != null && options.Delay.RateValue != null)
    {
      int rate = options.Delay.RateValue.Value;
      if (rate <= 0 || rate > 100)
      {
        _logger.LogError("Invalid rate value. It should be between 1 and 100.");
        await _next(context);
        return;
      }

      Random random = new Random();
      int randomValue = random.Next(1, 101);

      if (randomValue > rate)
      {
        await _next(context);
        return;
      }

      int minimumLatency = options.Delay.MinimumLatencyValue.Value;
      int maximumLatency = options.Delay.MaximumLatencyValue.Value;
      int delayRange = maximumLatency - minimumLatency;

      if (delayRange < 0)
      {
        _logger.LogError("Invalid latency range. Maximum latency must be greater than or equal to minimum latency.");
        await _next(context);
        return;
      }

      double delay = (double)options.Delay.MinimumLatencyValue + (delayRange * new Random().NextDouble());
      await Task.Delay(TimeSpan.FromMilliseconds(delay));
    }

    var originalBodyStream = context.Response.Body;

    using var memoryStream = new MemoryStream();
    context.Response.Body = memoryStream;

    await _next(context);

    if (options?.StatusCodeValue != null)
    {
      context.Response.StatusCode = (int)options.StatusCodeValue;
      memoryStream.Seek(0, SeekOrigin.Begin);
      await memoryStream.CopyToAsync(originalBodyStream);
      context.Response.Body = originalBodyStream;
      return;
    }
  }
}

