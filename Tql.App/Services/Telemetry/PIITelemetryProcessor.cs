using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Tql.App.Services.Telemetry;

internal class PIITelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public PIITelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(Microsoft.ApplicationInsights.Channel.ITelemetry item)
    {
        item.Context.Cloud.RoleInstance = null;
        item.Context.Cloud.RoleName = null;

        _next.Process(item);
    }
}
