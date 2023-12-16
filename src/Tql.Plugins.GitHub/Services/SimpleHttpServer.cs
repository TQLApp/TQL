using System.Collections.Specialized;
using System.Net;
using System.Security;
using Microsoft.Extensions.Logging;

namespace Tql.Plugins.GitHub.Services;

internal class SimpleHttpServer : IDisposable
{
    private readonly ILogger _logger;
    private readonly HttpListener _http;

    public SimpleHttpServer(string prefix, ILogger logger)
    {
        _logger = logger;
        _http = new HttpListener();

        _http.Prefixes.Add(prefix);

        logger.LogInformation("Listing for incoming requests");

        _http.Start();
    }

    public async Task<NameValueCollection> GetResponse(TimeSpan timeout)
    {
        var contextTask = _http.GetContextAsync();

        if (await Task.WhenAny(contextTask, Task.Delay(timeout)) != contextTask)
            throw new GitHubOAuthException("Authentication timed out");

        var context = await contextTask;

        var buffer = Encoding
            .UTF8
            .GetBytes(
                $"<html><body>{SecurityElement.Escape(Labels.GitHubOAuthWorkflow_AuthenticationComplete)}</body></html>"
            );

        context.Response.ContentLength64 = buffer.Length;

        await using var responseOutput = context.Response.OutputStream;

        await responseOutput.WriteAsync(buffer, 0, buffer.Length);

        return context.Request.QueryString;
    }

    public void Dispose()
    {
        _http.Stop();

        _logger.LogInformation("HTTP server stopped");

        ((IDisposable)_http).Dispose();
    }
}
