using Microsoft.Extensions.Logging;

namespace SteadybitFaultInjection.Injections;

public static class InjectionResolver
{
    public static ISteadybitInjection? ResolveInjection(
        this SteadybitInjectionOptions options,
        IEnumerable<ISteadybitInjection> injections
    )
    {
        if (options.Injection == null)
        {
            return null;
        }

        return injections.FirstOrDefault(injection =>
            injection
                .GetType()
                .Name.Equals(options.Injection, StringComparison.CurrentCultureIgnoreCase)
            || injection
                .GetType()
                .Name.StartsWith(options.Injection, StringComparison.OrdinalIgnoreCase)
        );
    }

    public static bool IsValid(this SteadybitInjectionOptions options, ILogger logger)
    {
        if (!options.EnabledValue)
        {
            logger.LogDebug("Steadybit Fault Injection is disabled. Middleware won't be executed.");
            return false;
        }

        if (!InjectionHelper.IsValidRate(options.RateValue))
        {
            logger.LogWarning(
                "Key Steadybit:FaultInjection:Rate is not provided or invalid. Middleware won't be executed."
            );
            return false;
        }

        if (!InjectionHelper.ShouldExecuteBasedOnRate((int)options.RateValue!, out int rateValue))
        {
            logger.LogWarning(
                "Rate is not met ({RateValue} <= {Rate}). Middleware won't be executed.",
                options.RateValue,
                rateValue
            );
            return false;
        }

        if (options?.Revision == null)
        {
            logger.LogWarning(
                "Sentinel key Steadybit:FaultInjection:Revision is missing. Middleware won't be executed."
            );
            return false;
        }

        return true;
    }
}
