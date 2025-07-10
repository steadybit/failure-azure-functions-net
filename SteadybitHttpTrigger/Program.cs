using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using SteadybitFailureInjection;

var builder = FunctionsApplication.CreateBuilder(args);

string endpoint = "https://failureinjectionconfiguration.azconfig.io";

builder.Configuration.AddAzureAppConfiguration(options =>
{
  options.Connect(new Uri(endpoint), new DefaultAzureCredential()).ConfigureSteadybitFailureInjection();
});

builder.Services.AddAzureAppConfiguration();

builder.Services.AddFeatureManagement();

builder.Services.AddSteadybitFailureServices();

builder.UseAzureAppConfiguration();

builder.UseMiddleware<SteadybitInjectionMiddleware>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
