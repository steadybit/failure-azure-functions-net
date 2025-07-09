
using Microsoft.AspNetCore.Http;

namespace SteadybitFailureInjection.Failures;

public class StatusCodeFailure : ISteadybitFailure, IDisposable
{
  private Stream? _startingBodyStream;
  private Stream? _newStream;
  public int Priority => 100; // Should always be executed last.

  public void Dispose()
  {
    if (_newStream != null)
    {
      _newStream.Dispose();
    }
  }

  public Task ExecuteBeforeAsync(HttpContext context, SteadybitFailureOptions options)
  {
    _startingBodyStream = context.Response.Body;
    var memoryStream = new MemoryStream();
    context.Response.Body = memoryStream;
    return Task.CompletedTask;
  }
  public async Task ExecuteAfterAsync(HttpContext context, SteadybitFailureOptions options)
  {
    if (options?.StatusCodeValue != null && _newStream != null && _startingBodyStream != null)
    {
      context.Response.StatusCode = (int)options.StatusCodeValue;
      _newStream.Seek(0, SeekOrigin.Begin);
      await _newStream.CopyToAsync(_startingBodyStream);
      context.Response.Body = _startingBodyStream;
      return;
    }
  }
}