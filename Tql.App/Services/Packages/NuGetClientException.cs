namespace Tql.App.Services.Packages;

internal class NuGetClientException : Exception
{
    public NuGetClientException() { }

    public NuGetClientException(string message)
        : base(message) { }

    public NuGetClientException(string message, Exception innerException)
        : base(message, innerException) { }
}
