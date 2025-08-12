using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;

namespace SteadybitFaultInjection.Injections;

public class StatusCodeFailure : ISteadybitInjection
{
    private HttpRequestData? _httpRequestData;

    private readonly ILogger _logger;

    public HttpRequestData? HttpRequestData
    {
        get => _httpRequestData;
        private set => _httpRequestData = value;
    }

    public StatusCodeFailure(ILogger<StatusCodeFailure> logger)
    {
        _logger = logger;
    }

    public virtual async Task<HttpRequestData?> GetHttpRequestDataAsync(FunctionContext context)
    {
        return await context.GetHttpRequestDataAsync();
    }

    public async Task ExecuteBeforeAsync(
        ISteadybitContext context,
        SteadybitInjectionOptions options
    )
    {
        var ctx = context.Unwrap();
        if (ctx is FunctionContext fnContext)
        {
            var request = await GetHttpRequestDataAsync(fnContext);
            HttpRequestData = request;
        }
        else if (context is HttpContext)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }

    public Task ExecuteAfterAsync(ISteadybitContext context, SteadybitInjectionOptions options)
    {
        if (HttpRequestData == null)
        {
            _logger.LogWarning(
                "HttpRequestData is not present, might not be using HTTP Trigger, skipping injection..."
            );
            return Task.CompletedTask;
        }

        if (options.StatusCodeValue == null)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:StatusCode is not provided or invalid, skipping injection..."
            );
            return Task.CompletedTask;
        }

        var ctx = context.Unwrap();
        if (ctx is FunctionContext fnContext)
        {
            var response = fnContext.GetHttpResponseData();

            if (response != null)
            {
                var customResponse = HttpRequestData.CreateResponse(
                    (HttpStatusCode)options.StatusCodeValue
                );
                customResponse.Headers = response.Headers;
                customResponse.Body = response.Body;

                fnContext.GetInvocationResult().Value = customResponse;
                _logger.LogInformation(
                    $"Injected status code: {options.StatusCodeValue.GetType().Name} ({options.StatusCodeValue})"
                );
            }

            return Task.CompletedTask;
        }
        else if (ctx is HttpContext httpContext)
        {
            throw new NotImplementedException("Not implemented.");
        }

        return Task.CompletedTask;
    }
}
