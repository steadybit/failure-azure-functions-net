using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace SteadybitFaultInjection;

public interface ISteadybitContext
{
    public object Unwrap { get; }
}

public class SteadybitHttpContext(HttpContext context) : ISteadybitContext
{
    public HttpContext Context { get; set; } = context;

    public object Unwrap => Context;
}

public class SteadybitFunctionContext(FunctionContext context) : ISteadybitContext
{
    public FunctionContext Context { get; set; } = context;

    public object Unwrap => Context;
}
