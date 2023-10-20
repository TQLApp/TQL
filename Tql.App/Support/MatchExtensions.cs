using Tql.Abstractions;
using Tql.App.Services.Telemetry;

namespace Tql.App.Support;

internal static class TypeIdExtensions
{
    public static void InitializeTelemetry(this MatchTypeId self, ITelemetry telemetry)
    {
        telemetry.AddProperty(nameof(self.Id), self.Id.ToString());
        telemetry.AddProperty(nameof(self.PluginId), self.PluginId.ToString());
    }
}
