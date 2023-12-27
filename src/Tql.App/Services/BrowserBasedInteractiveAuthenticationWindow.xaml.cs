using System.Collections.Specialized;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services;

internal partial class BrowserBasedInteractiveAuthenticationWindow
{
    private readonly string _resourceName;
    private readonly string _loginUrl;
    private readonly string _redirectUrl;
    private readonly IStore _store;
    private readonly ILogger<BrowserBasedInteractiveAuthenticationWindow> _logger;
    private SimpleHttpServer? _server;
    private readonly TaskCompletionSource<BrowserBasedInteractiveAuthenticationResult> _tcs;

    public BrowserBasedInteractiveAuthenticationWindow(
        string resourceName,
        string loginUrl,
        string redirectUrl,
        TaskCompletionSource<BrowserBasedInteractiveAuthenticationResult> tcs,
        IStore store,
        ILogger<BrowserBasedInteractiveAuthenticationWindow> logger
    )
    {
        _resourceName = resourceName;
        _loginUrl = loginUrl;
        _redirectUrl = redirectUrl;
        _tcs = tcs;
        _store = store;
        _logger = logger;

        InitializeComponent();

        _plugin.Text = resourceName;
    }

    private async void BaseWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var redirectUri = new Uri(_redirectUrl);

            var prefix = $"{redirectUri.Scheme}://{redirectUri.Authority}/";

            _server = new SimpleHttpServer(prefix, _logger);

            _server.RequestReceived += _server_RequestReceived;

            var resourceNameHash = Utilities.Encryption.Sha1Hash(_resourceName);
            var browserCacheFolder = Path.Combine(
                ((Store)_store).CacheFolder,
                "Browser Based Interactive Authentication",
                resourceNameHash
            );
            Directory.CreateDirectory(browserCacheFolder);

            var environment = await CoreWebView2Environment.CreateAsync(null, browserCacheFolder);

            await _webView.EnsureCoreWebView2Async(environment);

            _webView.Source = new Uri(_loginUrl);
        }
        catch (Exception ex)
        {
            _tcs.SetException(ex);

            Close();
        }
    }

    private void _server_RequestReceived(object? sender, SimpleHttpRequestEventArgs e)
    {
        Dispatcher.BeginInvoke(() => HandleRequest(e.Uri, e.QueryString));
    }

    private void HandleRequest(Uri uri, NameValueCollection queryString)
    {
        _tcs.SetResult(new BrowserBasedInteractiveAuthenticationResult(uri, queryString));

        Close();
    }

    private void BaseWindow_Unloaded(object sender, RoutedEventArgs e)
    {
        _server?.Dispose();
    }
}
