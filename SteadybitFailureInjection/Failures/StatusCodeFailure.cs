using System.ComponentModel;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace SteadybitFailureInjection.Failures;

public class SteadybitException : Exception
{
  public SteadybitException(string message) : base(message)
  {
  }
}

public class StatusCodeFailure : ISteadybitFailure
{
  private HttpRequestData? _httpRequestData;
  public int Priority => 100; // Should always be executed last.

  public async Task ExecuteBeforeAsync(FunctionContext context, SteadybitFailureOptions options)
  {
  }


  public async Task ExecuteAfterAsync(FunctionContext context, SteadybitFailureOptions options)
  {
  }
}