namespace Tql.App.Services.Telemetry;

internal interface IDependencyTelemetry : ITelemetry
{
    bool IsSuccess { get; set; }
    string? ResultCode { get; set; }
}
