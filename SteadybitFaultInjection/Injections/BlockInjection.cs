using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using SteadybitFaultInjection;

namespace SteadybitFailureInjection.Failures;

public class BlockInjection : ISteadybitInjectionWithTermination
{
    public bool ShouldTerminate { get; set; } = false;

    public async Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request == null)
        {
            ShouldTerminate = false;
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

        // Console.WriteLine($"BlockInjection: Host = {host ?? "null"}");
        // Console.WriteLine(
        //     $"BlockInjection: HostsValue = {(options.Block?.HostsValue != null ? string.Join(", ", options.Block.HostsValue) : "null")}"
        // );
        if (
            host != null
            && options.Block != null
            && options.Block.HostsValue.Count() > 0
            && options.Block.HostsValue.Contains(host)
        )
        {
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
            ShouldTerminate = true;
        }
    }

    public Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        ShouldTerminate = false;
        return Task.CompletedTask;
    }
}
