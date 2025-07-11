using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using SteadybitFaultInjection;

namespace SteadybitFaultInjections.Injections;

public interface ISteadybitInjection
{
    Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options);
    Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options);
}

public interface ISteadybitInjectionWithTermination : ISteadybitInjection
{
    bool ShouldTerminate { get; set; }
}
