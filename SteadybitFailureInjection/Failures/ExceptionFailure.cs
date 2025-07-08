using Microsoft.AspNetCore.Http;

namespace SteadybitFailureInjection.Failures;

public class ExceptionFailure : ISteadybitFailure
{
  public int Priority => 0;

  public Task ExecuteAsync(RequestDelegate next, HttpContext context, SteadybitFailureOptions options)
  {
      if (options.Exception?.Message == null)
      {
        return Task.CompletedTask;
      }

      throw new Exception(options.Exception.Message);
  }
}