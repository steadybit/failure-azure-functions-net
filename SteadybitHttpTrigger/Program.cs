using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SteadybitFaultInjection;

var builder = FunctionsApplication.CreateBuilder(args);

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

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddLogging(loggingBuilder =>
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    var telemetryConfiguration =
        serviceProvider.GetRequiredService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();
    var logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
        .CreateLogger();

    loggingBuilder.AddSerilog(logger, dispose: true);
});

builder.Services.AddAzureAppConfiguration();

builder.Services.AddSteadybitFailureServices();

builder.UseAzureAppConfiguration();

builder.UseMiddleware<SteadybitAzureFunctionsMiddleware>();

builder.Build().Run();
