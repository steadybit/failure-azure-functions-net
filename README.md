# Steadybit .NET Middleware for Azure Functions

## Description

Steadybit .NET Middleware for Azure Functions injects the faults during Azure Function execution.
It utilizes Azure App Configuration for managing the injection parameters.

## Prerequisites

- .NET 8+
- Azure Functions use Isolated Worker model
- No usage of ASP.NET abstractions in Azure Functions (required for status code injection)

## Setup

To use Steadybit .NET Middleware for Azure Functions the following NuGet package is required:

```
dotnet add package Steadybit.FaultInjection
```

### Azure Functions

In ```Program.cs``` add the following configuration:

```
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options
        .Connect(new Uri(endpoint), new DefaultAzureCredential())
        .ConfigureSteadybitFaultInjection();
});

builder.Services.AddSteadybitFailureServices();
builder.UseMiddleware<SteadybitInjectionMiddleware>();
```

### ASP.NET

In ```Program.cs``` add the following configuration:

```
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options
        .Connect(new Uri(endpoint), new DefaultAzureCredential())
        .ConfigureSteadybitFaultInjection();
});

builder.Services.AddSteadybitFailureServices();
builder.Services.AddAzureAppConfiguration();

var app = builder.Build();

app.UseAzureAppConfiguration();
app.UseMiddleware<SteadybitMiddleware>();
```

The middleware requires ```App Configuration Data Reader``` role to read the configuration entries from the Azure App Configuration.


## Example

Check out the example in the ```SteadybitHttpTrigger``` folder inside of this repository.
