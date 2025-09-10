using Microsoft.Extensions.Configuration;

namespace SteadybitFaultInjection;

public static class SteadybitOptionsBinder
{
    public static SteadybitInjectionOptions GetSteadybitFailureOptions(
        this IConfiguration configuration
    )
    {
        var options = new SteadybitInjectionOptions();
        var key =
            $"{SteadybitFaultInjectionConfigurator.SteadybitFaultInjectionsPrefix}{SteadybitFaultInjectionConfigurator.ResolveSuffix()}";
        IConfigurationSection section = configuration.GetSection(key);
        section.Bind(options);

        return options;
    }
}
