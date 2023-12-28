namespace Tql.App.Services.UIService;

internal class BrowserBasedInteractiveAuthenticationException : Exception
{
    public BrowserBasedInteractiveAuthenticationException() { }

    public BrowserBasedInteractiveAuthenticationException(string? message)
        : base(message) { }

    public BrowserBasedInteractiveAuthenticationException(
        string? message,
        Exception? innerException
    )
        : base(message, innerException) { }
}
