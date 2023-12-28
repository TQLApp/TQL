using System.Collections.Specialized;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services.UIService;

internal partial class BrowserBasedInteractiveAuthenticationWindow
{
    public class Factory(
        IStore store,
        IPluginManager pluginManager,
        ILogger<BrowserBasedInteractiveAuthenticationWindow> logger
    )
    {
        public BrowserBasedInteractiveAuthenticationWindow CreateInstance(
            InteractiveAuthenticationResource resource,
            string loginUrl,
            string redirectUrl,
            TaskCompletionSource<BrowserBasedInteractiveAuthenticationResult> tcs
        ) => new(resource, loginUrl, redirectUrl, tcs, store, pluginManager, logger);
    }

    private readonly InteractiveAuthenticationResource _resource;
    private readonly string _loginUrl;
    private readonly string _redirectUrl;
    private readonly IStore _store;
    private readonly ILogger<BrowserBasedInteractiveAuthenticationWindow> _logger;
    private SimpleHttpServer? _server;
    private readonly TaskCompletionSource<BrowserBasedInteractiveAuthenticationResult> _tcs;

    public BrowserBasedInteractiveAuthenticationWindow(
        InteractiveAuthenticationResource resource,
        string loginUrl,
        string redirectUrl,
        TaskCompletionSource<BrowserBasedInteractiveAuthenticationResult> tcs,
        IStore store,
        IPluginManager pluginManager,
        ILogger<BrowserBasedInteractiveAuthenticationWindow> logger
    )
    {
        _resource = resource;
        _loginUrl = loginUrl;
        _redirectUrl = redirectUrl;
        _tcs = tcs;
        _store = store;
        _logger = logger;

        InitializeComponent();

        var plugin = pluginManager.Plugins.Single(p => p.Id == resource.PluginId);

        _resourceName.Text = $"{plugin.Title} - {resource.ResourceName}";
        _resourceIcon.Source = resource.ResourceIcon;
    }

    private async void BaseWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var redirectUri = new Uri(_redirectUrl);

            var prefix = $"{redirectUri.Scheme}://{redirectUri.Authority}/";

            _server = new SimpleHttpServer(prefix, _logger);

            _server.RequestReceived += _server_RequestReceived;

            var browserCacheFolder = Path.Combine(
                _store.GetCacheFolder(_resource.PluginId),
                "Browser Based Interactive Authentication",
                _resource.ResourceId.ToString()
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
