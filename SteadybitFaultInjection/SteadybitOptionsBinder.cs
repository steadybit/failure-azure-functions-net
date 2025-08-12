using Microsoft.Extensions.Configuration;

namespace SteadybitFaultInjection;

public static class SteadybitOptionsBinder
{
    public static SteadybitInjectionOptions GetSteadybitFailureOptions(
        this IConfiguration configuration
    )
    {
        var options = new SteadybitInjectionOptions();
        IConfigurationSection section = configuration.GetSection("Steadybit:FaultInjection");
        section.Bind(options);

        return options;
    }
}
