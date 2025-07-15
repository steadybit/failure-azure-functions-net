using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteadybitFaultInjection;

namespace Steadybit.HttpTrigger;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IConfiguration _configuration;

    public HttpTrigger(ILogger<HttpTrigger> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function("HttpTrigger")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req
    )
    {
        var options = new SteadybitInjectionOptions();
        _configuration.GetSection("Steadybit:FaultInjection").Bind(options);
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync(JsonSerializer.Serialize(options));
        return response;
    }
}
