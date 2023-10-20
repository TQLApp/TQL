namespace Tql.App.Services.Telemetry;

internal interface IRequestTelemetry : ITelemetry
{
    bool IsSuccess { get; set; }
    string? RequestCode { get; set; }
}
