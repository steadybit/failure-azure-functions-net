using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using SteadybitFailureInjection.Failures;

public static class SteadybitFailureInjectionConfigurator
{
  public static string SteadybitFailureInjectionPrefix = "Steadybit:FaultInjection";
  public static string SteadybitFailureFeatureFlag = "SteadybitFaultInjectionEnabled";


  public static void ConfigureSteadybitFailureInjection(this AzureAppConfigurationOptions options)
  {
    options.Select($"{SteadybitFailureInjectionPrefix}:*", LabelFilter.Null)
    .ConfigureRefresh(refresh =>
    {
      refresh.Register($"{SteadybitFailureInjectionPrefix}:Revision", refreshAll: true).SetRefreshInterval(TimeSpan.FromSeconds(30));
    }).UseFeatureFlags(featureFlagOptions =>
    {
      featureFlagOptions.Select(SteadybitFailureFeatureFlag, LabelFilter.Null);
      featureFlagOptions.SetRefreshInterval(TimeSpan.FromSeconds(30));
    });
  }

  public static void AddSteadybitFailureServices(this IServiceCollection services)
  {
    services.AddScoped<ISteadybitFailure, DelayFailure>();
    services.AddScoped<ISteadybitFailure, ExceptionFailure>();
    services.AddScoped<ISteadybitFailure, StatusCodeFailure>();
  }
}