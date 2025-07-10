using Microsoft.AspNetCore.Rewrite;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using SteadybitFailureInjection.Failures;

namespace SteadybitFaultInjection;

public class SteadybitInjectionMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger _logger;
    private readonly IEnumerable<ISteadybitInjection> _injections;

    public SteadybitInjectionMiddleware(
        IConfiguration configuration,
        IFeatureManager featureManager,
        ILoggerFactory? loggerFactory,
        IEnumerable<ISteadybitInjection> injections
    )
    {
        _configuration = configuration;
        _featureManager = featureManager;
        _logger =
            loggerFactory?.CreateLogger<SteadybitInjectionMiddleware>()
            ?? NullLoggerFactory.Instance.CreateLogger<SteadybitInjectionMiddleware>();
        _injections = injections;
    }

    public async Task<bool> IsMiddlewareEnabledAsync()
    {
        return await _featureManager.IsEnabledAsync("SteadybitFaultInjectionEnabled");
    }

    public bool IsValidInjection(
        SteadybitInjectionOptions options,
        IEnumerable<ISteadybitInjection> injections
    )
    {
        if (options.Injection == null)
        {
            return false;
        }

        return !injections.Any(injection =>
            nameof(injection).ToLower() == options.Injection.ToLower()
            || (
                nameof(injection).Contains("Injection")
                && nameof(injection).Split("Injection")[0].ToLower() == options.Injection.ToLower()
            )
        );
    }

    public SteadybitInjectionOptions GetSteadybitFailureOptionsAsync()
    {
        var options = new SteadybitInjectionOptions();
        IConfigurationSection section = _configuration.GetSection("Steadybit:FaultInjection");
        section.Bind(options);

        return options;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        if (!await IsMiddlewareEnabledAsync())
        {
            await next(context);
            return;
        }

        var options = GetSteadybitFailureOptionsAsync();

        if (!IsValidInjection(options, _injections))
        {
            await next(context);
            _logger.LogWarning(
                "Key Steadybit:FaultInjection:Injection is not set or invalid. Middleware won't be executed."
            );
            return;
        }

        if (options?.Revision == null)
        {
            await next(context);
            _logger.LogWarning(
                "Sentinel key Steadybit:FaultInjection:Revision is not set. Configuration won't be able to refresh."
            );
            return;
        }

        foreach (var failure in _injections)
        {
            await failure.ExecuteBeforeAsync(context, options);

            if (
                failure is ISteadybitInjectionWithTermination injectionWithTermination
                && injectionWithTermination.ShouldTerminate
            )
            {
                await next(context);
                return;
            }
        }

        await next(context);

        foreach (var failure in _injections)
        {
            await failure.ExecuteAfterAsync(context, options);
        }
    }
}
