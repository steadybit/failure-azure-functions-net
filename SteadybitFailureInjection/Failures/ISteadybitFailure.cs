using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace SteadybitFailureInjection.Failures;

public interface ISteadybitFailure
{
  int Priority { get; }
  Task ExecuteBeforeAsync(FunctionContext context, SteadybitFailureOptions options);
  Task ExecuteAfterAsync(FunctionContext context, SteadybitFailureOptions options);
}