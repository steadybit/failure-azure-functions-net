using Microsoft.Extensions.Logging;

namespace SteadybitFaultInjection.Injections;

public class FillDiskInjection(ILogger<FillDiskInjection> logger) : ISteadybitInjection
{
    private readonly ILogger<FillDiskInjection> _logger = logger;

    public Task ExecuteAfterAsync(ISteadybitContext _, SteadybitInjectionOptions options)
    {
        if (options?.FillDisk == null || options.FillDisk.MegabytesValue == null)
        {
            _logger.LogWarning(
                "Key Steadybit:Injection:FillDisk:Megabytes is not provided or invalid, skipping injection..."
            );
            return Task.CompletedTask;
        }

        var tempPath = Path.GetTempPath();
        var tempFilePath = Path.Join(tempPath, "Steadybit GmbH", $"fill-disk-{Guid.NewGuid()}.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath) ?? tempPath);
        var fileStream = File.OpenWrite(tempFilePath);
        byte[] buffer = new byte[1024 * 1024];

        for (int i = 0; i < options.FillDisk.MegabytesValue; i++)
        {
            fileStream.Write(buffer, 0, buffer.Length);
        }

        _logger.LogInformation(
            "Injected disk fill of {Megabytes} MB at {TempFilePath}.",
            options.FillDisk.MegabytesValue, tempFilePath
        );

        return Task.CompletedTask;
    }

    public Task ExecuteBeforeAsync(ISteadybitContext _, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }
}
