using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Test;

/// <summary>
/// Runs the client against a live YouTrack, read-only (GET requests only),
/// when YOUTRACK_TEST_URL and YOUTRACK_TEST_TOKEN are set. Ignored otherwise.
/// </summary>
[TestFixture]
internal class LiveYouTrackFixture
{
    private HttpClient _httpClient = null!;
    private string _serviceUrl = null!;
    private string _token = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var url = Environment.GetEnvironmentVariable("YOUTRACK_TEST_URL");
        var token = Environment.GetEnvironmentVariable("YOUTRACK_TEST_TOKEN");

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(token))
        {
            Assert.Ignore(
                "Set YOUTRACK_TEST_URL and YOUTRACK_TEST_TOKEN to run against a live YouTrack."
            );
            return;
        }

        _serviceUrl = YouTrackUrls.NormalizeServiceUrl(url);
        _token = token;
        _httpClient = new HttpClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _httpClient?.Dispose();
    }

    private YouTrackClient CreateClient(string? token = null) =>
        new(_httpClient, _serviceUrl, token ?? _token);

    [Test]
    public async Task CheckServiceUrlAcceptsTheServiceUrl()
    {
        var result = await YouTrackClient.CheckServiceUrl(_httpClient, _serviceUrl);

        Assert.That(result.ServiceUrl, Is.EqualTo(_serviceUrl));
        Assert.That(result.IsSuggestion, Is.False);
        Assert.That(result.Version, Is.Not.Empty);
    }

    [Test]
    public async Task CheckServiceUrlOffersTheServiceUrlForACopiedPage()
    {
        var result = await YouTrackClient.CheckServiceUrl(_httpClient, $"{_serviceUrl}issues");

        Assert.That(result.ServiceUrl, Is.EqualTo(_serviceUrl));
        Assert.That(result.IsSuggestion, Is.True);
    }

    [Test]
    public void ACopiedPageIsNotAYouTrackApi()
    {
        Assert.ThrowsAsync<YouTrackNotAnApiException>(
            () => new YouTrackClient(_httpClient, $"{_serviceUrl}issues", _token).GetCurrentUser()
        );
    }

    [Test]
    public async Task GetCurrentUser()
    {
        var user = await CreateClient().GetCurrentUser();

        Assert.That(user.Login, Is.Not.Empty);
        Assert.That(user.Guest, Is.False);
    }

    [Test]
    public void AnInvalidTokenIsUnauthorized()
    {
        Assert.ThrowsAsync<YouTrackUnauthorizedException>(
            () => CreateClient("perm:invented.token.value").GetCurrentUser()
        );
    }

    [Test]
    public async Task SearchIssuesAndGetIssue()
    {
        var client = CreateClient();

        var issues = await client.SearchIssues("sort by: updated desc", 5);

        Assert.That(issues, Has.Length.InRange(1, 5));
        Assert.That(
            issues.Select(p => p.IdReadable),
            Has.All.Matches<string>(YouTrackQueries.IsIssueId)
        );

        var issue = await client.GetIssue(issues[0].IdReadable.ToLowerInvariant());

        Assert.That(issue, Is.EqualTo(issues[0]));
    }

    [Test]
    public async Task GetIssueOfAnUnknownIdIsNull()
    {
        Assert.That(await CreateClient().GetIssue("NOSUCHPROJECT-999999999"), Is.Null);
    }

    [Test]
    public void AHalfTypedQueryIsAnInvalidQuery()
    {
        Assert.ThrowsAsync<YouTrackInvalidQueryException>(
            () => CreateClient().SearchIssues("#NoSuchStateOrTagAtAll")
        );
    }

    [Test]
    public async Task CachedListsPage()
    {
        var client = new YouTrackClient(_httpClient, _serviceUrl, _token) { PageSize = 2 };

        var projects = await client.GetProjects();

        Assert.That(projects, Is.Not.Empty);
        Assert.That(projects, Has.None.Matches<YouTrackProjectDto>(p => p.Archived));
        Assert.That(projects.Select(p => p.Id), Is.Unique);

        await client.GetAgileBoards();
        await client.GetSavedQueries();
        await client.GetDashboards();
    }

    [Test]
    public async Task SearchUsers()
    {
        var client = CreateClient();
        var me = await client.GetCurrentUser();

        var users = await client.SearchUsers(me.Login);

        Assert.That(
            users,
            Has.All.Matches<YouTrackUserDto>(p =>
                !string.IsNullOrEmpty(p.Email) && !p.Banned && !p.Guest
            )
        );
    }
}
