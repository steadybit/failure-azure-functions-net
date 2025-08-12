using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SteadybitFaultInjection;
using SteadybitFaultInjection.Injections;

namespace SteadybitFaultInjections.Injections.Tests
{
    public class StatusCodeFailureTests
    {
        private readonly Mock<ILogger<StatusCodeFailure>> _loggerMock;
        private readonly Mock<FunctionContext> _contextMock;
        private readonly Mock<HttpRequestData> _requestMock;

        public StatusCodeFailureTests()
        {
            _loggerMock = new Mock<ILogger<StatusCodeFailure>>();
            _contextMock = new Mock<FunctionContext>();
            _requestMock = new Mock<HttpRequestData>(_contextMock.Object);
        }

        [Fact]
        public async Task Test_StatusCodeInjection_ShouldSetHttpRequestData()
        {
            var injection = new Mock<StatusCodeFailure>(_loggerMock.Object);
            var options = new SteadybitInjectionOptions();

            injection
                .Setup(x => x.GetHttpRequestDataAsync(_contextMock.Object))
                .ReturnsAsync(_requestMock.Object);

            await injection.Object.ExecuteBeforeAsync(
                new SteadybitFunctionContext(_contextMock.Object),
                options
            );

            Assert.NotNull(injection.Object.HttpRequestData);
        }

        [Fact]
        public async Task Test_StatusCodeInjection_ShouldSkipIfHttpDataIsNull()
        {
            var injection = new StatusCodeFailure(_loggerMock.Object);
            var options = new SteadybitInjectionOptions { StatusCode = "404" };

            await injection.ExecuteAfterAsync(
                new SteadybitFunctionContext(_contextMock.Object),
                options
            );

            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) => v.ToString()!.Contains("HttpRequestData is not present")
                        ),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task Test_StatusCodeInjection_ShouldSkipIfStatusCodeIsNull()
        {
            var injection = new Mock<StatusCodeFailure>(_loggerMock.Object);
            var options = new SteadybitInjectionOptions { StatusCode = null };

            injection
                .Setup(x => x.GetHttpRequestDataAsync(_contextMock.Object))
                .ReturnsAsync(_requestMock.Object);

            await injection.Object.ExecuteBeforeAsync(
                new SteadybitFunctionContext(_contextMock.Object),
                options
            );
            await injection.Object.ExecuteAfterAsync(
                new SteadybitFunctionContext(_contextMock.Object),
                options
            );

            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) => v.ToString()!.Contains("Steadybit:Injection:StatusCode")
                        ),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeast(1)
            );
        }
    }
}
