using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using SteadybitFaultInjection.Injections;
using SteadybitFaultInjections.Injections;

namespace SteadybitFaultInjection;

public class SteadybitInjectionMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IEnumerable<ISteadybitInjection> _injections;

    public SteadybitInjectionMiddleware(
        IConfiguration configuration,
        ILogger<SteadybitInjectionMiddleware> logger,
        IEnumerable<ISteadybitInjection> injections
    )
    {
        _configuration = configuration;
        _logger = logger;
        _injections = injections;
    }

    public ISteadybitInjection? GetInjectionToExecute(
        SteadybitInjectionOptions options,
        IEnumerable<ISteadybitInjection> injections
    )
    {
        if (options.Injection == null)
        {
            return null;
        }

        _logger.LogDebug($"Looking for injection with name: {options.Injection}");

        foreach (var injection in injections)
        {
            _logger.LogDebug($"Injection exists: {injection.GetType().Name}");
        }

        return injections.FirstOrDefault(injection =>
            injection.GetType().Name.ToLower() == options.Injection.ToLower()
            || injection
                .GetType()
                .Name.StartsWith(options.Injection, StringComparison.OrdinalIgnoreCase)
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
        var options = GetSteadybitFailureOptionsAsync();

        if (!options.EnabledValue)
        {
            _logger.LogDebug(
                "Steadybit Fault Injection is disabled. Middleware won't be executed."
            );
            await next(context);
            return;
        }

        if (!InjectionHelper.IsValidRate(options.RateValue))
        {
            _logger.LogWarning(
                "Key Steadybit:FaultInjection:Rate is not provided or invalid, skipping injection..."
            );
            await next(context);
            return;
        }

        if (!InjectionHelper.ShouldExecuteBasedOnRate((int)options.RateValue!, out int rateValue))
        {
            _logger.LogWarning(
                $"Rate is not met (<= {rateValue}), skipping delay injection. Rate: {options.RateValue}."
            );
            await next(context);
            return;
        }

        var injection = GetInjectionToExecute(options, _injections);

        if (injection == null)
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

        await injection.ExecuteBeforeAsync(context, options);

        if (
            injection is ISteadybitInjectionWithTermination injectionWithTermination
            && injectionWithTermination.ShouldTerminate
        )
        {
            await next(context);
            return;
        }

        await next(context);

        await injection.ExecuteAfterAsync(context, options);
    }
}
