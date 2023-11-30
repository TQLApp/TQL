using Microsoft.ApplicationInsights.Extensibility;

namespace Tql.App.Services.Telemetry;

internal class PIITelemetryProcessor(ITelemetryProcessor next) : ITelemetryProcessor
{
    public void Process(Microsoft.ApplicationInsights.Channel.ITelemetry item)
    {
        item.Context.Cloud.RoleInstance = null;
        item.Context.Cloud.RoleName = null;

        next.Process(item);
    }
}
