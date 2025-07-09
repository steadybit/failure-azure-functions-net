using Microsoft.AspNetCore.Http;

namespace SteadybitFailureInjection.Failures;

public interface ISteadybitFailure
{
  int Priority { get; }
  Task ExecuteBeforeAsync(HttpContext context, SteadybitFailureOptions options);
  Task ExecuteAfterAsync(HttpContext context, SteadybitFailureOptions options);
}