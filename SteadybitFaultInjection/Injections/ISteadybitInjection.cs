
using Microsoft.Azure.Functions.Worker;
using SteadybitFaultInjection;

namespace SteadybitFailureInjection.Failures;

public interface ISteadybitInjection
{
  int Priority { get; }
  Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options);
  Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options);
}