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

builder.Services.AddSteadybitFailureServices();

builder.Services.AddAzureAppConfiguration();

var app = builder.Build();

app.UseAzureAppConfiguration();

app.MapGet("/", () => "Hello World!");

app.UseMiddleware<SteadybitMiddleware>();

app.Run();
