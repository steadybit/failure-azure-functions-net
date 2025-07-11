using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjections.Injections;

public class BlockInjection : ISteadybitInjectionWithTermination
{
    private readonly ILogger<BlockInjection> _logger;
    public bool ShouldTerminate { get; set; } = false;

    public BlockInjection(ILogger<BlockInjection> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request == null)
        {
            _logger.LogDebug(
                "HttpRequestData is not present, might not be using HTTP Trigger, skipping injection..."
            );
            return;
        }

        string? host = null;
        foreach (var header in request.Headers)
        {
            if (header.Key == "Host")
            {
                host = header.Value.FirstOrDefault();
            }
        }

        if (host == null)
        {
            _logger.LogWarning(
                "Host header is not present in the request, skipping block injection."
            );
            return;
        }

        if (options?.Block?.Hosts == null || options.Block.HostsValue.Count() == 0)
        {
            _logger.LogWarning(
                "Keys Steadybit:Injection:Block:Hosts option is not provided or invalid, skipping block injection..."
            );
            return;
        }

        if (!options.Block.HostsValue.Contains(host))
        {
            _logger.LogDebug($"Host '{host}' is not in the block list, skipping block injection.");
            return;
        }

        var customResponse = request.CreateResponse(HttpStatusCode.Forbidden);
        var body = new MemoryStream();
        body.Write(
            System.Text.Encoding.UTF8.GetBytes(
                "This request has been blocked by Steadybit Fault Injection Middleware."
            )
        );
        body.Seek(0, SeekOrigin.Begin);
        customResponse.Body = body;

        context.GetInvocationResult().Value = customResponse;
        _logger.LogInformation($"Injected block response for host: {host}.");
        ShouldTerminate = true;
    }

    public Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }
}
