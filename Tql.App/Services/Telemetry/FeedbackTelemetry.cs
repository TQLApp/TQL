namespace Tql.App.Services.Telemetry;

internal record FeedbackTelemetry(
    FeedbackType Type,
    string? EmailAddress,
    string Message,
    string? SystemInformation,
    ImmutableArray<string>? EnabledPlugins
);

internal enum FeedbackType
{
    Bug,
    FeatureRequest
}
