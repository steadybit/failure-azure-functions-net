using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace SteadybitFailureInjection.Failures;

public class ExceptionFailure : ISteadybitFailure
{
  public int Priority => 0;

  public Task ExecuteAfterAsync(FunctionContext context, SteadybitFailureOptions options)
  {
    return Task.CompletedTask;
  }

  public Task ExecuteBeforeAsync(FunctionContext context, SteadybitFailureOptions options)
  {
      if (options.Exception?.Message == null)
      {
        return Task.CompletedTask;
      }

      throw new Exception(options.Exception.Message);
  }
}