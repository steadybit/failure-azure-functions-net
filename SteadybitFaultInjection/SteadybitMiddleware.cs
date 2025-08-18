using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection;

public class SteadybitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public SteadybitMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<SteadybitAzureFunctionsMiddleware> logger
    )
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IEnumerable<ISteadybitInjection> injections)
    {
        var options = _configuration.GetSteadybitFailureOptions();

        if (!options.IsValid(_logger))
        {
            await _next(context);
            return;
        }

        var injection = options.ResolveInjection(injections);

        if (injection == null)
        {
            _logger.LogWarning(
                "Key Steadybit:FaultInjection:Injection is not set or invalid. Middleware won't be executed."
            );
            await _next(context);
            return;
        }

        ISteadybitContext ctx = new SteadybitHttpContext(context);

        await injection.ExecuteBeforeAsync(ctx, options);

        if (
            injection is ISteadybitInjectionWithTermination injectionWithTermination
            && injectionWithTermination.ShouldTerminate
        )
        {
            await _next(context);
            return;
        }

        await _next(context);

        await injection.ExecuteAfterAsync(ctx, options);
    }
}
