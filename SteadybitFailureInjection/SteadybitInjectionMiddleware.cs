using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using SteadybitFailureInjection.Failures;

namespace SteadybitFailureInjection;

public class SteadybitInjectionMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IConfiguration _configuration;
  private readonly IFeatureManager _featureManager;
  private readonly ILogger _logger;

  private readonly IEnumerable<ISteadybitFailure> _failures;

  public SteadybitInjectionMiddleware(RequestDelegate next, IConfiguration configuration, IFeatureManager featureManager, ILoggerFactory? loggerFactory, IEnumerable<ISteadybitFailure> failures)
  {
    _next = next;
    _configuration = configuration;
    _featureManager = featureManager;
    _failures = failures.OrderBy(f => f.Priority);
    _logger = loggerFactory?.CreateLogger<SteadybitInjectionMiddleware>() ?? NullLoggerFactory.Instance.CreateLogger<SteadybitInjectionMiddleware>();
  }

  public async Task<bool> IsMiddlewareEnabledAsync()
  {
    return await _featureManager.IsEnabledAsync("SteadybitFaultInjectionEnabled");
  }

  public SteadybitFailureOptions GetSteadybitFailureOptionsAsync()
  {
    var options = new SteadybitFailureOptions();
    IConfigurationSection section = _configuration.GetSection("Steadybit:FaultInjection");
    section.Bind(options);

    return options;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    if (!await IsMiddlewareEnabledAsync())
    {
      await _next(context);
      return;
    }

    var options = GetSteadybitFailureOptionsAsync(); 

    if (options?.Revision == null)
    {
      await _next(context);
      _logger.LogWarning("Sentinel key Steadybit:FaultInjection:Revision is not set. Configuration won't be able to refresh.");
      return;
    }

    foreach (var failure in _failures)
    {
        Console.WriteLine($"Executing failure: {failure.GetType().Name} with priority {failure.Priority}");
        await failure.ExecuteAsync(_next, context, options);
    }

    await _next(context);
  }
}

