using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjections.Injections;

public class FillDiskInjection : ISteadybitInjection
{
    private readonly ILogger<FillDiskInjection> _logger;

    public FillDiskInjection(ILogger<FillDiskInjection> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
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
            $"Injected disk fill of {options.FillDisk.MegabytesValue} MB at {tempFilePath}."
        );

        return Task.CompletedTask;
    }

    public Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }
}
