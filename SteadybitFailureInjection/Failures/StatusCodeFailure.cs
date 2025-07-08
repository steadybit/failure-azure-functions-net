
using Microsoft.AspNetCore.Http;

namespace SteadybitFailureInjection.Failures;

public class StatusCodeFailure : ISteadybitFailure
{
  public int Priority => 100; // Should always be executed last.

  public async Task ExecuteAsync(RequestDelegate next, HttpContext context, SteadybitFailureOptions options)
  {
    var originalBodyStream = context.Response.Body;

    using var memoryStream = new MemoryStream();
    context.Response.Body = memoryStream;

    await next(context);

    if (options?.StatusCodeValue != null)
    {
      context.Response.StatusCode = (int)options.StatusCodeValue;
      memoryStream.Seek(0, SeekOrigin.Begin);
      await memoryStream.CopyToAsync(originalBodyStream);
      context.Response.Body = originalBodyStream;
      return;
    }
  }

  public bool ShouldApply(SteadybitFailureOptions options)
  {
    return options.StatusCodeValue.HasValue;
  }
}