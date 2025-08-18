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
    private Stream? _requestStream;
    private Stream? _responseStream;

    private readonly ILogger _logger;

    public HttpRequestData? HttpRequestData
    {
        get => _httpRequestData;
        private set => _httpRequestData = value;
    }

    public Stream? RequestStream
    {
        get => _requestStream;
        private set => _requestStream = value;
    }

    public Stream? ResponseStream
    {
        get => _requestStream;
        private set => _requestStream = value;
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
        else if (ctx is HttpContext httpContext)
        {
            _requestStream = httpContext.Response.Body;
            _responseStream = new MemoryStream();
            httpContext.Response.Body = _responseStream;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public async Task ExecuteAfterAsync(
        ISteadybitContext context,
        SteadybitInjectionOptions options
    )
    {
        if (options.StatusCodeValue == null)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:StatusCode is not provided or invalid, skipping injection..."
            );
            return;
        }

        var ctx = context.Unwrap();

        if (ctx is FunctionContext fnContext)
        {
            if (HttpRequestData == null)
            {
                _logger.LogWarning(
                    "HttpRequestData is not present, might not be using HTTP Trigger, skipping injection..."
                );
                return;
            }
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

            return;
        }
        else if (ctx is HttpContext httpContext)
        {
            if (_requestStream == null)
            {
                _logger.LogError("Request stream is null, skipping injection...");
                return;
            }

            if (_responseStream == null)
            {
                _logger.LogError("Response stream is null, skipping injection...");
                return;
            }

            httpContext.Response.StatusCode = (int)options.StatusCodeValue;
            _responseStream.Seek(0, SeekOrigin.Begin);
            await _responseStream.CopyToAsync(_requestStream);
            httpContext.Response.Body = _responseStream;
        }
        else
        {
            throw new NotImplementedException();
        }

        return;
    }
}
