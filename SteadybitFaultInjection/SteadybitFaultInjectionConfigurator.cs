using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using SteadybitFaultInjections.Injections;

public static class SteadybitFaultInjectionConfigurator
{
    public static string SteadybitFaultInjectionsPrefix = "Steadybit:FaultInjection";

    public static void ConfigureSteadybitFaultInjection(this AzureAppConfigurationOptions options)
    {
        options
            .Select($"{SteadybitFaultInjectionsPrefix}:Enabled", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:Rate", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:Injection", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:Revision", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:StatusCode", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:Delay:MinimumLatency", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:Delay:MaximumLatency", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:Exception:Message", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:FillDisk:Megabytes", LabelFilter.Null)
            .Select($"{SteadybitFaultInjectionsPrefix}:Block:Hosts", LabelFilter.Null)
            .ConfigureRefresh(refresh =>
            {
                refresh
                    .Register($"{SteadybitFaultInjectionsPrefix}:Revision", refreshAll: true)
                    .SetRefreshInterval(TimeSpan.FromSeconds(30));
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
}
