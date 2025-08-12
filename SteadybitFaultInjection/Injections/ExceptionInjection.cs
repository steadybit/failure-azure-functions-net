using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection.Injections;

public class SteadybitException : Exception
{
    public SteadybitException() { }

    public SteadybitException(string message)
        : base(message) { }

    public SteadybitException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

public class ExceptionInjection : ISteadybitInjection
{
    private readonly ILogger<ExceptionInjection> _logger;

    public ExceptionInjection(ILogger<ExceptionInjection> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAfterAsync(ISteadybitContext _, SteadybitInjectionOptions options)
    {
        if (options?.Exception?.Message == null)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:Exception:Message is not provided, skipping injection..."
            );
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Injecting exception with message: \"{Message}\".",
            options.Exception.Message
        );

        throw new SteadybitException(options.Exception.Message);
    }

    public Task ExecuteBeforeAsync(ISteadybitContext _, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }
}
