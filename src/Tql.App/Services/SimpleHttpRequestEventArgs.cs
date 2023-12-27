using System.Collections.Specialized;

namespace Tql.App.Services;

internal class SimpleHttpRequestEventArgs(Uri uri, NameValueCollection queryString)
{
    public Uri Uri { get; } = uri;
    public NameValueCollection QueryString { get; } = queryString;
}
