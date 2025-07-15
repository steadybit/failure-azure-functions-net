using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjections.Injections;

public class ExceptionInjection : ISteadybitInjection
{
    private readonly ILogger<ExceptionInjection> _logger;

    public ExceptionInjection(ILogger<ExceptionInjection> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        if (options?.Exception?.Message == null)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:Exception:Message is not provided, skipping injection..."
            );
            return Task.CompletedTask;
        }

        _logger.LogWarning(
            "Injecting exception with message: \"{Message}\".",
            options.Exception.Message
        );
        throw new Exception(options.Exception.Message);
    }

    public Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }
}
