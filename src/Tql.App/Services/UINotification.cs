namespace Tql.App.Services;

internal record UINotification(string Key, string Message, Action? Activate, Action? Dismiss);
