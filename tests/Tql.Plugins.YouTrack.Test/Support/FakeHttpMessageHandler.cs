using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tql.Plugins.YouTrack.Test.Support;

/// <summary>
/// Replays canned answers by exact request URL and records every request. A
/// URL without an answer gets a 599, so a test sees which URL it missed.
/// </summary>
internal class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpResponseMessage>> _answers = new();

    public List<RecordedRequest> Requests { get; } = new();

    public FakeHttpMessageHandler Answer(
        string url,
        string content,
        string mediaType,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        _answers[new Uri(url).AbsoluteUri] = () =>
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, mediaType)
            };

        return this;
    }

    public FakeHttpMessageHandler AnswerJson(
        string url,
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK
    ) => Answer(url, json, "application/json", statusCode);

    public FakeHttpMessageHandler AnswerFixture(
        string url,
        string fixture,
        HttpStatusCode statusCode = HttpStatusCode.OK
    ) => AnswerJson(url, Fixtures.Read(fixture), statusCode);

    public FakeHttpMessageHandler AnswerHtml(string url) =>
        Answer(url, Fixtures.Read("WebApp.html"), "text/html");

    public FakeHttpMessageHandler AnswerBytes(string url, byte[] content, string mediaType)
    {
        _answers[new Uri(url).AbsoluteUri] = () =>
        {
            var result = new ByteArrayContent(content);
            result.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = result };
        };

        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var url = request.RequestUri!.AbsoluteUri;

        Requests.Add(new RecordedRequest(request.Method, url, request.Headers.Authorization));

        var response = _answers.TryGetValue(url, out var answer)
            ? answer()
            : new HttpResponseMessage((HttpStatusCode)599)
            {
                Content = new StringContent($"No answer for {url}")
            };

        response.RequestMessage = request;

        return Task.FromResult(response);
    }
}

internal record RecordedRequest(
    HttpMethod Method,
    string Url,
    AuthenticationHeaderValue? Authorization
);
