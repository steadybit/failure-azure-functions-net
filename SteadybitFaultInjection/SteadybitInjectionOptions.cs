using System.Net;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Client;

namespace SteadybitFaultInjection;

public class SteadybitInjectionOptions
{
    public string? Revision { get; set; }
    public string? Injection { get; set; }

    public SteadybitDelayInjectionOptions? Delay { get; set; }

    public SteadybitExceptionInjectionOptions? Exception { get; set; }

    public SteadybitBlockInjectionOptions? Block { get; set; }

    public SteadybitFillDiskInjectionOptions? FillDisk { get; set; }

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
            if (
                int.TryParse(_statusCode, out int statusCodeValue)
                && Enum.IsDefined(typeof(HttpStatusCode), statusCodeValue)
            )
            {
                return (HttpStatusCode)statusCodeValue;
            }

            return null;
        }
    }

    public string? Enabled { get; set; }

    public bool EnabledValue
    {
        get
        {
            if (string.IsNullOrEmpty(Enabled))
            {
                return false;
            }

            return Enabled.Equals("true", StringComparison.OrdinalIgnoreCase)
                || Enabled.Equals("1", StringComparison.OrdinalIgnoreCase)
                || Enabled.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }

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
}

public class SteadybitDelayInjectionOptions
{
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

public class SteadybitExceptionInjectionOptions
{
    private string? _message;
    public string? Message
    {
        get => _message;
        set => _message = value;
    }
}

public class SteadybitBlockInjectionOptions
{
    string? _hosts;

    public string? Hosts
    {
        get => _hosts;
        set => _hosts = value;
    }

    public IEnumerable<string> HostsValue
    {
        get
        {
            if (string.IsNullOrEmpty(_hosts))
            {
                return Array.Empty<string>();
            }

            return _hosts
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(host => host.Trim())
                .Where(host => !string.IsNullOrEmpty(host));
        }
    }
}

public class SteadybitFillDiskInjectionOptions
{
    string? _megabytes;

    public string? Megabytes
    {
        get => _megabytes;
        set => _megabytes = value;
    }

    public int? MegabytesValue
    {
        get
        {
            if (int.TryParse(_megabytes, out int megabytesValue) && megabytesValue > 0)
            {
                return megabytesValue;
            }
            return null;
        }
    }
}
