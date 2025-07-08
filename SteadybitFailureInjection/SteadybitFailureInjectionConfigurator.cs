using Microsoft.Extensions.Configuration.AzureAppConfiguration;

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
}