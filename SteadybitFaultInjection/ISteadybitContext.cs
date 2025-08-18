using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Identity.Client;

namespace SteadybitFaultInjection;

public interface ISteadybitContext
{
    public object Unwrap();
}

public class SteadybitHttpContext : ISteadybitContext
{
    public HttpContext _context { get; set; }

    public SteadybitHttpContext(HttpContext context)
    {
        _context = context;
    }

    public object Unwrap()
    {
        return _context;
    }
}

public class SteadybitFunctionContext : ISteadybitContext
{
    public FunctionContext _context { get; set; }

    public SteadybitFunctionContext(FunctionContext context)
    {
        _context = context;
    }

    public object Unwrap()
    {
        return _context;
    }
}
