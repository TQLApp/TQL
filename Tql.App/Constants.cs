using JetBrains.Annotations;
using NuGet.Frameworks;

namespace Tql.App;

internal static class Constants
{
    public const int MaxPenalty = 5;
    public const int MaxDistance = 3;
    public const string HelpUrl = "https://github.com/pvginkel/TQL/wiki";

    public const string BugReportUrl =
        "https://github.com/pvginkel/TQL/issues/new?assignees=&labels=&projects=&template=bug_report.md&title=";
    public const string FeatureRequestUrl =
        "https://github.com/pvginkel/TQL/issues/new?assignees=&labels=&projects=&template=feature_request.md&title=";

    [UsedImplicitly]
    public const string ApplicationInsightsDevelopmentConnectionString =
        "InstrumentationKey=3df0ad73-a682-404b-9adc-3360168a98df;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/";

    [UsedImplicitly]
    public const string ApplicationInsightsConnectionString =
        "InstrumentationKey=b24fa90c-fa1d-4b19-836b-66202920fa50;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/";

    public const string PackageSource =
        "https://pvginkel.pkgs.visualstudio.com/Launcher/_packaging/TQLPlugins/nuget/v3/index.json";

    public static readonly Guid PackageManagerConfigurationId = Guid.Parse(
        "8dd1ea3b-d412-43c3-b5a4-a36bb7d52189"
    );
    public static readonly Guid PackageManagerPageId = Guid.Parse(
        "4474c041-702c-415a-8531-5460dc1b9db1"
    );

    public static NuGetFramework ApplicationFrameworkVersion =
        new(FrameworkConstants.FrameworkIdentifiers.Net, new Version(4, 8));
}
