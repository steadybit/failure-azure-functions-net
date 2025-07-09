using Microsoft.AspNetCore.Http;

namespace SteadybitFailureInjection.Failures;

public class ExceptionFailure : ISteadybitFailure
{
  public int Priority => 0;

  public Task ExecuteAfterAsync(HttpContext context, SteadybitFailureOptions options)
  {
    return Task.CompletedTask;
  }

  public Task ExecuteBeforeAsync(HttpContext context, SteadybitFailureOptions options)
  {
      if (options.Exception?.Message == null)
      {
        return Task.CompletedTask;
      }

      throw new Exception(options.Exception.Message);
  }
}