namespace Tql.App.Services.Telemetry;

internal interface ITelemetry : IDisposable
{
    void AddProperty(string name, string value);
}
