using JetBrains.Annotations;
using NuGet.Frameworks;

namespace Tql.App;

internal static class Constants
{
    public static readonly TimeSpan SynchronizationInterval = TimeSpan.FromHours(1);
    public static readonly TimeSpan SynchronizationRestoreDelay = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan TermsOfServiceCheckInterval = TimeSpan.FromDays(1);

    public const int MaxPenalty = 5;
    public const int MaxDistance = 3;
    public const string HelpUrl = "https://github.com/TQLApp/TQL/wiki";

    public const string BugReportUrl =
        "https://github.com/TQLApp/TQL/issues/new?assignees=&labels=&projects=&template=bug_report.md&title=";
    public const string FeatureRequestUrl =
        "https://github.com/TQLApp/TQL/issues/new?assignees=&labels=&projects=&template=feature_request.md&title=";

    [UsedImplicitly]
    public const string ApplicationInsightsDevelopmentConnectionString =
        "InstrumentationKey=3df0ad73-a682-404b-9adc-3360168a98df;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/";

    [UsedImplicitly]
    public const string ApplicationInsightsConnectionString =
        "InstrumentationKey=b24fa90c-fa1d-4b19-836b-66202920fa50;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/";

    public const string TermsOfServiceUrl = "https://tqlapp.dev/Terms-of-service.html";

    public static readonly Guid SynchronizationPageId = Guid.Parse(
        "b2560909-5210-4efa-bfd2-08cb1df96d09"
    );

    public static readonly Guid GeneralPageId = Guid.Parse("df92b623-a629-465a-bddf-8f36ef6d4fdd");

    public static readonly Guid PluginsPageId = Guid.Parse("96260cfa-5814-4ed4-ac69-fcc63e4f4571");

    public static readonly ImmutableArray<string> PackageSources = ImmutableArray.Create(
        "https://api.nuget.org/v3/index.json"
    );

    public static readonly Guid SettingsConfigurationId = Guid.Parse(
        "30817a0a-ef4f-4f0e-b765-35a5e375e12e"
    );
    public static readonly Guid PackageManagerConfigurationId = Guid.Parse(
        "8dd1ea3b-d412-43c3-b5a4-a36bb7d52189"
    );
    public static readonly Guid PackageSourcesPageId = Guid.Parse(
        "4474c041-702c-415a-8531-5460dc1b9db1"
    );
    public static readonly Guid ProfilesPageId = Guid.Parse("c4bb3afb-a39a-48b4-81d3-6fd65270ba78");
    public static readonly Guid PackageStoreConfigurationId = Guid.Parse(
        "52b8c0d3-d66e-4b98-a127-4ebb5c0a516d"
    );

    public static NuGetFramework ApplicationFrameworkVersion = new NuGetFramework(
        FrameworkConstants.FrameworkIdentifiers.NetCoreApp,
        new Version(8, 0),
        FrameworkConstants.PlatformIdentifiers.Windows,
        new Version(7, 0)
    );

    public static ImmutableArray<string> SystemPackageIds = ImmutableArray.Create(
        "TQLApp.Abstractions",
        "TQLApp.Utilities"
    );
}
