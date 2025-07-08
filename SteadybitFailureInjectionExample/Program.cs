using SteadybitFailureInjection;
using Microsoft.Azure.AppConfiguration.AspNetCore;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

//string endpoint = builder.Configuration.GetValue<string>("AzureAppConfiguration:Endpoint") ?? throw new InvalidOperationException("The setting `Endpoints:AppConfiguration` was not found.");
string endpoint = "https://failureinjectionconfiguration.azconfig.io";
Console.WriteLine($"Using Azure App Configuration endpoint: {endpoint}");

builder.Configuration.AddAzureAppConfiguration(options =>
{
  options.Connect(new Uri(endpoint), new DefaultAzureCredential()).ConfigureSteadybitFailureInjection();
});

builder.Services.AddAzureAppConfiguration();

builder.Services.AddFeatureManagement();

builder.Services.AddHttpClient();

builder.Services.AddSteadybitFailureServices();

var app = builder.Build();

app.UseAzureAppConfiguration();

app.UseMiddleware<SteadybitInjectionMiddleware>();

app.MapGet("/", () => $"Hello World!");

Console.WriteLine($"Running...");
app.Run();
