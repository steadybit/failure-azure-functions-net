using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using SteadybitFaultInjections.Injections;

namespace SteadybitFaultInjection.Tests;

public class FillDiskInjectionTests
{
    private readonly Mock<ILogger<FillDiskInjection>> _logger;
    private readonly Mock<FunctionContext> _context;

    public FillDiskInjectionTests()
    {
        _logger = new Mock<ILogger<FillDiskInjection>>();
        _context = new Mock<FunctionContext>();
    }

    [Fact]
    public async Task Test_ExceptionInjection_SkipsInjectionIfMessageIsMissing()
    {
        var exceptionInjection = new FillDiskInjection(_logger.Object);
        var options = new SteadybitInjectionOptions
        {
            FillDisk = new SteadybitFillDiskInjectionOptions { Megabytes = null },
        };

        await exceptionInjection.ExecuteBeforeAsync(_context.Object, options);
        await exceptionInjection.ExecuteAfterAsync(_context.Object, options);

        _logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Steadybit:Injection:FillDisk:Megabytes")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
