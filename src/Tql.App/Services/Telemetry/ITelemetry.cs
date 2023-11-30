namespace Tql.App.Services.Telemetry;

internal interface ITelemetry : IDisposable
{
    bool IsEnabled { get; }

    void AddProperty(string name, string value);
}
