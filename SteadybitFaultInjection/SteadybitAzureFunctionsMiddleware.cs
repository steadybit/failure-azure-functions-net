using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection;

public class SteadybitAzureFunctionsMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IEnumerable<ISteadybitInjection> _injections;

    public SteadybitAzureFunctionsMiddleware(
        IConfiguration configuration,
        ILogger<SteadybitAzureFunctionsMiddleware> logger,
        IEnumerable<ISteadybitInjection> injections
    )
    {
        _configuration = configuration;
        _logger = logger;
        _injections = injections;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var options = _configuration.GetSteadybitFailureOptions();

        if (options.IsValid(_logger))
        {
            await next(context);
            return;
        }

        var injection = options.ResolveInjection(_injections);

        if (injection == null)
        {
            _logger.LogWarning(
                "Key Steadybit:FaultInjection:Injection is not set or invalid. Middleware won't be executed."
            );
            await next(context);
            return;
        }

        ISteadybitContext ctx = new SteadybitFunctionContext(context);

        await injection.ExecuteBeforeAsync(ctx, options);

        if (
            injection is ISteadybitInjectionWithTermination injectionWithTermination
            && injectionWithTermination.ShouldTerminate
        )
        {
            await next(context);
            return;
        }

        await next(context);

        await injection.ExecuteAfterAsync(ctx, options);
    }
}
