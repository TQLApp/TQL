using System.Collections.Specialized;

namespace Tql.App.Services.UIService;

internal class SimpleHttpRequestEventArgs(Uri uri, NameValueCollection queryString)
{
    public Uri Uri { get; } = uri;
    public NameValueCollection QueryString { get; } = queryString;
}
