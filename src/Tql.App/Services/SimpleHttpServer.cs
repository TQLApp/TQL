using System.Net;
using Microsoft.Extensions.Logging;
using Tql.App.Support;

namespace Tql.App.Services;

internal class SimpleHttpServer : IDisposable
{
    private readonly ILogger _logger;
    private readonly HttpListener _http;

    public event EventHandler<SimpleHttpRequestEventArgs>? RequestReceived;

    public SimpleHttpServer(string prefix, ILogger logger)
    {
        _logger = logger;
        _http = new HttpListener();

        _http.Prefixes.Add(prefix);

        logger.LogInformation("Listing for incoming requests");

        _http.Start();

        TaskUtils.RunBackground(GetRequest);
    }

    private async Task GetRequest()
    {
        try
        {
            var context = await _http.GetContextAsync();

            var e = new SimpleHttpRequestEventArgs(
                context.Request.Url!,
                context.Request.QueryString
            );

            context.Response.ContentLength64 = 0;
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            context.Response.StatusDescription = "No Content";

            context.Response.Close();

            OnRequestReceived(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while waiting for request");
        }
    }

    public void Dispose()
    {
        _http.Stop();

        _logger.LogInformation("HTTP server stopped");

        ((IDisposable)_http).Dispose();
    }

    protected virtual void OnRequestReceived(SimpleHttpRequestEventArgs e) =>
        RequestReceived?.Invoke(this, e);
}
