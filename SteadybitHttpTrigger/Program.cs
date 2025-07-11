using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using SteadybitFaultInjection;

var builder = FunctionsApplication.CreateBuilder(args);

string endpoint = "https://failureinjectionconfiguration.azconfig.io";

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options
        .Connect(new Uri(endpoint), new DefaultAzureCredential())
        .ConfigureSteadybitFaultInjection();
});

builder.Services.AddAzureAppConfiguration();

builder.Services.AddFeatureManagement();

builder.Services.AddSteadybitFailureServices();

var config = builder.Configuration;

builder.UseMiddleware<SteadybitInjectionMiddleware>();

builder.UseAzureAppConfiguration();

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
