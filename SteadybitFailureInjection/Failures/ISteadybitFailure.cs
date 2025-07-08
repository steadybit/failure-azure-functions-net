using Microsoft.AspNetCore.Http;

namespace SteadybitFailureInjection.Failures;

public interface ISteadybitFailure
{
  int Priority { get; }
  Task ExecuteAsync(RequestDelegate next, HttpContext context, SteadybitFailureOptions options);
}