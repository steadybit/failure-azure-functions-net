using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection.Tests;

public class DelayInjectionTests
{
    private readonly Mock<ILogger<DelayInjection>> _logger;
    public readonly Mock<ISteadybitContext> _context;

    public DelayInjectionTests()
    {
        _logger = new Mock<ILogger<DelayInjection>>();
        _context = new Mock<ISteadybitContext>();
    }

    [Fact]
    public async Task Test_DelayInjection_DelaysInRange()
    {
        var delayInjection = new DelayInjection(_logger.Object);

        var options = new SteadybitInjectionOptions
        {
            Delay = new SteadybitDelayInjectionOptions
            {
                MinimumLatency = "500",
                MaximumLatency = "1000",
            },
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await delayInjection.ExecuteBeforeAsync(_context.Object, options);
        stopwatch.Stop();
        await delayInjection.ExecuteAfterAsync(_context.Object, options);

        Assert.InRange(
            (int)stopwatch.ElapsedMilliseconds,
            (int)options.Delay.MinimumLatencyValue!,
            (int)options.Delay.MaximumLatencyValue!
        );
    }

    [Fact]
    public async Task Test_DelayInjection_SkipDelayIfDelayOptionIsNull()
    {
        var delayInjection = new DelayInjection(_logger.Object);

        var options = new SteadybitInjectionOptions { Delay = null };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await delayInjection.ExecuteBeforeAsync(_context.Object, options);
        stopwatch.Stop();
        _logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Delay options are not provided")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
        await delayInjection.ExecuteAfterAsync(_context.Object, options);

        Assert.True(stopwatch.ElapsedMilliseconds < 30, "there should be no delay");
    }

    [Fact]
    public async Task Test_DelayInjection_SkipDelayIfMinimumLatencyIsNotSpecified()
    {
        var delayInjection = new DelayInjection(_logger.Object);

        var options = new SteadybitInjectionOptions
        {
            Delay = new SteadybitDelayInjectionOptions
            {
                MinimumLatency = null,
                MaximumLatency = "500",
            },
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await delayInjection.ExecuteBeforeAsync(_context.Object, options);
        stopwatch.Stop();
        _logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Steadybit:Injection:Delay:MinimumLatency")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
        await delayInjection.ExecuteAfterAsync(_context.Object, options);

        Assert.True(stopwatch.ElapsedMilliseconds < 30, "there should be no delay");
    }

    [Fact]
    public async Task Test_DelayInjection_SkipDelayIfMaximumLatencyIsNotSpecified()
    {
        var delayInjection = new DelayInjection(_logger.Object);

        var options = new SteadybitInjectionOptions
        {
            Delay = new SteadybitDelayInjectionOptions
            {
                MinimumLatency = "500",
                MaximumLatency = null,
            },
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await delayInjection.ExecuteBeforeAsync(_context.Object, options);
        stopwatch.Stop();
        _logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Steadybit:Injection:Delay:MaximumLatency")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
        await delayInjection.ExecuteAfterAsync(_context.Object, options);

        Assert.True(stopwatch.ElapsedMilliseconds < 30, "there should be no delay");
    }

    [Fact]
    public async Task Test_DelayInjection_SkipDelayIfMinimumLatencyIsGreaterThanMaximumLatency()
    {
        var delayInjection = new DelayInjection(_logger.Object);

        var options = new SteadybitInjectionOptions
        {
            Delay = new SteadybitDelayInjectionOptions
            {
                MinimumLatency = "500",
                MaximumLatency = "250",
            },
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await delayInjection.ExecuteBeforeAsync(_context.Object, options);
        stopwatch.Stop();
        _logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("must be greater than or equal")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
        await delayInjection.ExecuteAfterAsync(_context.Object, options);

        Assert.True(stopwatch.ElapsedMilliseconds < 30, "there should be no delay");
    }
}
