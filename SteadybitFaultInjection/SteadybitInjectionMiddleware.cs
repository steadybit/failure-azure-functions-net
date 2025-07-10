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
    private readonly IEnumerable<ISteadybitInjection> _failures;

    public SteadybitInjectionMiddleware(
        IConfiguration configuration,
        IFeatureManager featureManager,
        ILoggerFactory? loggerFactory,
        IEnumerable<ISteadybitInjection> failures
    )
    {
        _configuration = configuration;
        _featureManager = featureManager;
        _logger =
            loggerFactory?.CreateLogger<SteadybitInjectionMiddleware>()
            ?? NullLoggerFactory.Instance.CreateLogger<SteadybitInjectionMiddleware>();
        _failures = failures;
    }

    public async Task<bool> IsMiddlewareEnabledAsync()
    {
        return await _featureManager.IsEnabledAsync("SteadybitFaultInjectionEnabled");
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

        if (options?.Revision == null)
        {
            await next(context);
            _logger.LogWarning(
                "Sentinel key Steadybit:FaultInjection:Revision is not set. Configuration won't be able to refresh."
            );
            return;
        }

        foreach (var failure in _failures)
        {
            Console.WriteLine(
                $"Executing before failure: {failure.GetType().Name} with priority {failure.Priority}"
            );
            await failure.ExecuteBeforeAsync(context, options);
        }

        await next(context);

        foreach (var failure in _failures)
        {
            Console.WriteLine(
                $"Executing after failure: {failure.GetType().Name} with priority {failure.Priority}"
            );
            await failure.ExecuteAfterAsync(context, options);
        }
    }
}
