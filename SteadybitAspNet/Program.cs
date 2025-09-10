using System.Net;
using System.Net.Sockets;
using Azure.Identity;
using Microsoft.Extensions.Primitives;
using Serilog;
using SteadybitFaultInjection;

var builder = WebApplication.CreateBuilder(args);

// STEADYBIT_FAULT_INJECTION_ENDPOINT is required for the discovery to work.
var endpoint = Environment.GetEnvironmentVariable("STEADYBIT_FAULT_INJECTION_ENDPOINT");

if (string.IsNullOrEmpty(endpoint))
{
    throw new InvalidOperationException(
        "STEADYBIT_FAULT_INJECTION_ENDPOINT environment variable is not set."
    );
}

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options
        .Connect(new Uri(endpoint), new DefaultAzureCredential())
        .ConfigureSteadybitFaultInjection();
});

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSerilog(
    (services, lc) =>
    {
        lc.MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(
                services.GetService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(),
                TelemetryConverter.Traces
            );
    }
);

builder.Services.AddSteadybitFailureServices();

builder.Services.AddAzureAppConfiguration();

var app = builder.Build();

app.UseAzureAppConfiguration();

app.UseMiddleware<SteadybitMiddleware>();

app.MapGet("/", () => "Hello World!");

app.MapGet(
    "/Env",
    (HttpContext context) =>
    {
        if (!context.Request.Query.TryGetValue("key", out StringValues key))
        {
            return Results.BadRequest($"Key '${key}' is not found.");
        }

        var env = Environment.GetEnvironmentVariable(key.ToString());

        if (env == null)
        {
            return Results.BadRequest($"Key '${key}' is not found.");
        }

        return Results.Ok(new { key = key.ToString(), value = env });
    }
);

app.MapGet(
    "/HttpTrigger",
    (IConfiguration configuration, ILogger<Program> logger) =>
    {
        var options = new SteadybitInjectionOptions();
        configuration.GetSection("Steadybit:FaultInjection").Bind(options);
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return Results.Ok(options);
    }
);

app.MapPost(
    "/HttpTrigger",
    (IConfiguration configuration, ILogger<Program> logger) =>
    {
        var options = new SteadybitInjectionOptions();
        configuration.GetSection("Steadybit:FaultInjection").Bind(options);
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return Results.Ok(options);
    }
);

app.MapGet(
    "/TestOutbound",
    async (HttpContext context, ILogger<Program> logger, CancellationToken cancellationToken) =>
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var rootDomain = context.Request.Query["rootDomain"].FirstOrDefault();
            if (rootDomain == null)
            {
                var errorResult = new
                {
                    Error = "Bad request",
                    ExceptionMessage = "'rootDomain' query parameter is missing.",
                };
                return Results.BadRequest(errorResult);
            }
            var response = await httpClient.GetAsync(
                $"https://{rootDomain}",
                cancellationToken: cancellationToken
            );
            var ipAddresses = await Dns.GetHostAddressesAsync(rootDomain, cancellationToken);
            var resolvedIp = ipAddresses.Length > 0 ? ipAddresses[0].ToString() : "IP not found";
            var result = new { StatusCode = (int)response.StatusCode, ResolvedIp = resolvedIp };
            return Results.Ok(result);
        }
        catch (HttpRequestException httpEx) when (httpEx.InnerException is SocketException socketEx)
        {
            var errorResult = new
            {
                Error = "Connection blocked or reset",
                ExceptionMessage = httpEx.Message,
                SocketError = socketEx.SocketErrorCode.ToString(),
            };
            return Results.Json(errorResult, statusCode: (int)HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException exception)
        {
            var errorResult = new
            {
                Error = "Request timed out",
                ExceptionMessage = exception.Message,
            };
            return Results.Json(errorResult, statusCode: (int)HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                Error = "An unexpected error occurred",
                ExceptionMessage = ex.Message,
            };
            return Results.Json(errorResult, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
);

app.Run();
