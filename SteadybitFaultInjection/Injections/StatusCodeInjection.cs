using System.Net;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjections.Injections;

public class SteadybitException : Exception
{
    public SteadybitException(string message)
        : base(message) { }
}

public class StatusCodeFailure : ISteadybitInjection
{
    private HttpRequestData? _httpRequestData;

    private readonly ILogger _logger;

    public StatusCodeFailure(ILogger<StatusCodeFailure> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request == null)
        {
            _logger.LogDebug(
                "HttpRequestData is not present, might not be using HTTP Trigger, skipping injection..."
            );
            return;
        }

        if (options.StatusCodeValue == null)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:StatusCode is not provided or invalid, skipping injection..."
            );
            return;
        }

        _httpRequestData = request;
    }

    public Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        if (options.StatusCodeValue == null || _httpRequestData == null)
        {
            return Task.CompletedTask;
        }

        var response = context.GetHttpResponseData();

        if (response != null)
        {
            var customResponse = _httpRequestData.CreateResponse(
                (HttpStatusCode)options.StatusCodeValue
            );
            customResponse.Headers = response.Headers;
            customResponse.Body = response.Body;

            context.GetInvocationResult().Value = customResponse;
            _logger.LogInformation(
                $"Injected status code: {options.StatusCodeValue.GetType().Name} ({options.StatusCodeValue})"
            );
        }

        return Task.CompletedTask;
    }
}
