using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using SteadybitFailureInjection.Failures;

public static class SteadybitFaultInjectionConfigurator
{
    public static string SteadybitFailureInjectionPrefix = "Steadybit:FaultInjection";
    public static string SteadybitFailureFeatureFlag = "SteadybitFaultInjectionEnabled";

    public static void ConfigureSteadybitFaultInjection(this AzureAppConfigurationOptions options)
    {
        options
            .Select($"{SteadybitFailureInjectionPrefix}:*", LabelFilter.Null)
            .ConfigureRefresh(refresh =>
            {
                refresh
                    .Register($"{SteadybitFailureInjectionPrefix}:Revision", refreshAll: true)
                    .SetRefreshInterval(TimeSpan.FromSeconds(30));
            })
            .UseFeatureFlags(featureFlagOptions =>
            {
                featureFlagOptions.Select(SteadybitFailureFeatureFlag, LabelFilter.Null);
                featureFlagOptions.SetRefreshInterval(TimeSpan.FromSeconds(30));
            });
    }

    public static void AddSteadybitFailureServices(this IServiceCollection services)
    {
        services.AddScoped<ISteadybitInjection, DelayFailure>();
        services.AddScoped<ISteadybitInjection, ExceptionInjection>();
        services.AddScoped<ISteadybitInjection, StatusCodeFailure>();
        services.AddScoped<ISteadybitInjection, BlockInjection>();
        services.AddScoped<ISteadybitInjection, FillDiskInjection>();
    }

    public static async Task<HttpResponseData> ReturnStatus(
        this HttpRequestData req,
        HttpStatusCode status,
        string body = null
    )
    {
        var result = req.CreateResponse(status);
        if (!string.IsNullOrEmpty(body))
        {
            await result.WriteStringAsync(body);
        }
        return result;
    }
}
