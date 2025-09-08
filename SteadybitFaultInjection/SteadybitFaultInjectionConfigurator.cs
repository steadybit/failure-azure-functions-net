using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection;

public static class SteadybitFaultInjectionConfigurator
{
    public static readonly string SteadybitFaultInjectionsPrefix = "Steadybit:FaultInjection";

    public static AzureAppConfigurationOptions ConfigureSteadybitFaultInjection(
        this AzureAppConfigurationOptions options
    )
    {
        string prefix = $"{SteadybitFaultInjectionsPrefix}{ResolveSuffix()}";

        return options
            .Select($"{prefix}", LabelFilter.Null)
            .ConfigureRefresh(refresh =>
            {
                refresh
                    .Register($"{prefix}:Revision", refreshAll: true)
                    .SetRefreshInterval(TimeSpan.FromSeconds(30));
            });
    }

    public static string ResolveSuffix()
    {
        var suffix = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");

        if (suffix != null)
        {
            return $":{suffix}";
        }

        suffix = Environment.GetEnvironmentVariable("CONTAINER_APP_NAME");

        if (suffix != null)
        {
            return $":{suffix}";
        }

        suffix = string.Empty;

        return suffix;
    }

    public static void AddSteadybitFailureServices(this IServiceCollection services)
    {
        services.AddScoped<ISteadybitInjection, DelayInjection>();
        services.AddScoped<ISteadybitInjection, ExceptionInjection>();
        services.AddScoped<ISteadybitInjection, StatusCodeFailure>();
        services.AddScoped<ISteadybitInjection, FillDiskInjection>();
    }
}
