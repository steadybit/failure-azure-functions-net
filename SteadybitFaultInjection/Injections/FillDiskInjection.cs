using Microsoft.Azure.Functions.Worker;
using Microsoft.Identity.Client;
using SteadybitFaultInjection;

namespace SteadybitFailureInjection.Failures;

public class FillDiskInjection : ISteadybitInjection
{
    public Task ExecuteAfterAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        if (options.FillDisk == null || options.FillDisk.MegabytesValue == null)
        {
            return Task.CompletedTask;
        }

        var tempPath = Path.GetTempPath();
        var tempFilePath = Path.Join(tempPath, "Steadybit GmbH", $"fill-disk-{Guid.NewGuid()}.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath) ?? tempPath);
        Console.WriteLine($"Creating file at: {tempFilePath}");
        var fileStream = File.OpenWrite(tempFilePath);
        byte[] buffer = new byte[1024 * 1024];

        for (int i = 0; i < options.FillDisk.MegabytesValue; i++)
        {
            fileStream.Write(buffer, 0, buffer.Length);
        }

        return Task.CompletedTask;
    }

    public Task ExecuteBeforeAsync(FunctionContext context, SteadybitInjectionOptions options)
    {
        return Task.CompletedTask;
    }
}
