using Tql.Abstractions;
using Tql.App.Services.Telemetry;

namespace Tql.App.Support;

internal static class MatchExtensions
{
    public static void InitializeTelemetry(this IMatch self, ITelemetry telemetry)
    {
        if (!telemetry.IsEnabled)
            return;

        var type = self.GetType();
        var assemblyName = type.Assembly.GetName();

        telemetry.AddProperty("Type", type.FullName!);
        telemetry.AddProperty("Assembly", assemblyName.Name);
        telemetry.AddProperty("Version", assemblyName.Version.ToString());
        telemetry.AddProperty(nameof(self.TypeId.Id), self.TypeId.Id.ToString());
        telemetry.AddProperty(nameof(self.TypeId.PluginId), self.TypeId.PluginId.ToString());
    }
}
