namespace Tql.Plugins.Outlook.Services;

internal class OutlookCacheUpdateException : Exception
{
    public OutlookCacheUpdateException() { }

    public OutlookCacheUpdateException(string message)
        : base(message) { }

    public OutlookCacheUpdateException(string message, Exception innerException)
        : base(message, innerException) { }
}
