namespace Tql.App.Services;

internal class WindowMessageIPCException : Exception
{
    public WindowMessageIPCException() { }

    public WindowMessageIPCException(string message)
        : base(message) { }

    public WindowMessageIPCException(string message, Exception innerException)
        : base(message, innerException) { }
}
