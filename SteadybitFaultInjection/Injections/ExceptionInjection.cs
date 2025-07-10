using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using SteadybitFaultInjection;

namespace SteadybitFailureInjection.Failures;

public class ExceptionInjection : ISteadybitInjection 
{
  public int Priority => 0;

  public Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
  {
    return Task.CompletedTask;
  }

  public Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
  {
      if (options.Exception?.Message == null)
      {
        return Task.CompletedTask;
      }

      throw new Exception(options.Exception.Message);
  }
}