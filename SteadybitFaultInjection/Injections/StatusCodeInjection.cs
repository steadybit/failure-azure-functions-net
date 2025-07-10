using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using SteadybitFaultInjection;

namespace SteadybitFailureInjection.Failures;

public class SteadybitException : Exception
{
    public SteadybitException(string message)
        : base(message) { }
}

public class StatusCodeFailure : ISteadybitInjection
{
    private HttpRequestData? _httpRequestData;
    public int Priority => 100; // Should always be executed last.

    public async Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        var request = await context.GetHttpRequestDataAsync();
        _httpRequestData = request;
    }

    public async Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        var response = context.GetHttpResponseData();
        if (_httpRequestData != null && options.StatusCodeValue.HasValue && response != null)
        {
            var customResponse = _httpRequestData.CreateResponse(
                (HttpStatusCode)options.StatusCodeValue
            );
            customResponse.Headers = response.Headers;
            customResponse.Body = response.Body;

            context.GetInvocationResult().Value = customResponse;
        }
    }
}
