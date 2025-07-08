using System.Net;

namespace SteadybitFailureInjection;

public class SteadybitFailureOptions
{
  public string Revision { get; set; }

  public SteadybitDelayFailureOptions? Delay { get; set; }

  public SteadybitExceptionFailureOptions? Exception { get; set; }

  private string? _statusCode;
  public string? StatusCode
  {
    get => _statusCode;
    set => _statusCode = value;
  }

  public HttpStatusCode? StatusCodeValue
  {
    get
    {
      if (int.TryParse(_statusCode, out int statusCodeValue) && Enum.IsDefined(typeof(HttpStatusCode), statusCodeValue))
      {
        return (HttpStatusCode)statusCodeValue;
      }

      return null;
    }
  }
}

public class SteadybitDelayFailureOptions
{
  private string? _rate;
  public string? Rate
  {
    get => _rate;
    set => _rate = value;
  }

  public int? RateValue
  {
    get
    {
      bool success = int.TryParse(_rate, out int rateValue);
      return success ? rateValue : null;
    }
  }

  private string? _minimumLatency;
  public string? MinimumLatency
  {
    get => _minimumLatency ?? string.Empty;
    set => _minimumLatency = value;
  }

  public int? MinimumLatencyValue
  {
    get
    {
      bool success = int.TryParse(_minimumLatency, out int minimumLatencyValue);
      return success ? minimumLatencyValue : null;
    }
  }

  private string? _maximumLatency;
  public string? MaximumLatency
  {
    get => _maximumLatency;
    set => _maximumLatency = value;
  }

  public int? MaximumLatencyValue
  {
    get
    {
      bool success = int.TryParse(_maximumLatency, out int maximumLatencyValue);
      return success ? maximumLatencyValue : null;
    }
  }
}

public class SteadybitExceptionFailureOptions
{
  private string? _message;
  public string? Message
  {
    get => _message;
    set => _message = value;
  }
}