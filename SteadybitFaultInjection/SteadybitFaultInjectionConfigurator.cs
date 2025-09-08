using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection;

public static class SteadybitFaultInjectionConfigurator
{
    public static readonly string SteadybitFaultInjectionsPrefix = "Steadybit:FaultInjection";

    public static void ConfigureSteadybitFaultInjection(this AzureAppConfigurationOptions options)
    {
        options
            .Select($"{SteadybitFaultInjectionsPrefix}:*", LabelFilter.Null)
            .ConfigureRefresh(refresh =>
            {
                refresh
                    .Register($"{SteadybitFaultInjectionsPrefix}:Revision", refreshAll: true)
                    .SetRefreshInterval(TimeSpan.FromSeconds(30));
            });
    }

    public static void AddSteadybitFailureServices(this IServiceCollection services)
    {
        services.AddScoped<ISteadybitInjection, DelayInjection>();
        services.AddScoped<ISteadybitInjection, ExceptionInjection>();
        services.AddScoped<ISteadybitInjection, StatusCodeFailure>();
        services.AddScoped<ISteadybitInjection, FillDiskInjection>();
    }
}
