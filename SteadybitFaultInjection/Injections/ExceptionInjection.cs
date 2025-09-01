using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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

    public Task ExecuteAfterAsync(ISteadybitContext context, SteadybitInjectionOptions options)
    {
        var ctx = context.Unwrap();

        if (ctx is FunctionContext)
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

        return Task.CompletedTask;
    }

    public Task ExecuteBeforeAsync(ISteadybitContext context, SteadybitInjectionOptions options)
    {
        var ctx = context.Unwrap();

        if (ctx is HttpContext)
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

        return Task.CompletedTask;
    }
}
