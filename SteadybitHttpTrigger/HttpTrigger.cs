using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;

namespace Steadybit.HttpTrigger;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IConfiguration _configuration;

    public HttpTrigger(ILogger<HttpTrigger> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function("HttpTrigger")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req
    )
    {
        var options = new SteadybitInjectionOptions();
        _configuration.GetSection("Steadybit:FaultInjection").Bind(options);
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync(JsonSerializer.Serialize(options));
        return response;
    }

    [Function("Env")]
    public async Task<HttpResponseData> Env(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req
    )
    {
        var envName = req.Query.Get("key");

        if (envName == null)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            var errorResult = new
            {
                Error = "Bad request",
                ExceptionMessage = "'key' query parameter is missing.",
            };
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(errorResult));
            return errorResponse;
        }
        var envValue = Environment.GetEnvironmentVariable(envName);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(
            JsonSerializer.Serialize(new { key = envName, value = envValue })
        );
        return response;
    }

    [Function("TestOutbound")]
    public async Task<HttpResponseData> Run2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        CancellationToken token
    )
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var rootDomain = req.Query.Get("rootDomain");
            if (rootDomain == null)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                var errorResult = new
                {
                    Error = "Bad request",
                    ExceptionMessage = "'rootDomain' query parameter is missing.",
                };
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(errorResult));
                return errorResponse;
            }
            var response = await httpClient.GetAsync(
                $"https://{rootDomain}",
                cancellationToken: token
            );
            var ipAddresses = await Dns.GetHostAddressesAsync(rootDomain, token);
            var resolvedIp = ipAddresses.Length > 0 ? ipAddresses[0].ToString() : "IP not found";
            var httpResponse = req.CreateResponse(response.StatusCode);
            var result = new { StatusCode = (int)response.StatusCode, ResolvedIp = resolvedIp };
            await httpResponse.WriteStringAsync(JsonSerializer.Serialize(result));
            return httpResponse;
        }
        catch (HttpRequestException httpEx) when (httpEx.InnerException is SocketException socketEx)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
            var errorResult = new
            {
                Error = "Connection blocked or reset",
                ExceptionMessage = httpEx.Message,
                SocketError = socketEx.SocketErrorCode.ToString(),
            };
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(errorResult));
            return errorResponse;
        }
        catch (TaskCanceledException exception)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.RequestTimeout);
            var errorResult = new
            {
                Error = "Request timed out",
                ExceptionMessage = exception.Message,
            };
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(errorResult));
            return errorResponse;
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            var errorResult = new
            {
                Error = "An unexpected error occurred",
                ExceptionMessage = ex.Message,
            };
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(errorResult));
            return errorResponse;
        }
    }
}
