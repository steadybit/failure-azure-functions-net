using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjection.Tests;

public class ExceptionInjectionTests
{
    private readonly Mock<ILogger<ExceptionInjection>> _logger;
    public readonly Mock<SteadybitHttpContext> _context;

    public readonly Mock<HttpContext> _fnContext;

    public ExceptionInjectionTests()
    {
        _logger = new Mock<ILogger<ExceptionInjection>>();
        _fnContext = new Mock<HttpContext>();
        _context = new Mock<SteadybitHttpContext>(_fnContext.Object);
    }

    [Fact]
    public async Task Test_ExceptionInjection_ThrowsException()
    {
        var exceptionInjection = new ExceptionInjection(_logger.Object);
        var options = new SteadybitInjectionOptions
        {
            Exception = new SteadybitExceptionInjectionOptions
            {
                Message = "Exception raised for testing purposes.",
            },
        };

        await Assert.ThrowsAsync<SteadybitException>(async () =>
        {
            await exceptionInjection.ExecuteBeforeAsync(_context.Object, options);
        });
    }

    [Fact]
    public async Task Test_ExceptionInjection_SkipsInjectionIfMessageIsMissing()
    {
        var exceptionInjection = new ExceptionInjection(_logger.Object);
        var options = new SteadybitInjectionOptions
        {
            Exception = new SteadybitExceptionInjectionOptions { Message = null },
        };

        await exceptionInjection.ExecuteBeforeAsync(_context.Object, options);
        await exceptionInjection.ExecuteAfterAsync(_context.Object, options);

        _logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Steadybit:Injection:Exception:Message")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
