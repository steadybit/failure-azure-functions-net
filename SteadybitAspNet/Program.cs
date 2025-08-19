using Azure.Identity;
using Serilog;
using SteadybitFaultInjection;

var builder = WebApplication.CreateBuilder(args);

var endpoint = Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_ENDPOINT");

if (string.IsNullOrEmpty(endpoint))
{
    throw new InvalidOperationException(
        "AZURE_APP_CONFIG_ENDPOINT environment variable is not set."
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

app.Run();
