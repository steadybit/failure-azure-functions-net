namespace SteadybitFaultInjection.Injections;

public interface ISteadybitInjection
{
    Task ExecuteBeforeAsync(ISteadybitContext context, SteadybitInjectionOptions options);
    Task ExecuteAfterAsync(ISteadybitContext context, SteadybitInjectionOptions options);
}

public interface ISteadybitInjectionWithTermination : ISteadybitInjection
{
    bool ShouldTerminate { get; set; }
}
